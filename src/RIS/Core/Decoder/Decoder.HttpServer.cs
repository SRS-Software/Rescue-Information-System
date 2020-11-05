#region

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using RestSharp;
using RIS.Net.Http;
using RIS.SharpHttpServer;
using SRS.Utilities;
using SRS.Utilities.Extensions;

#endregion

namespace RIS.Core.Decoder
{
    public class DecoderHttpServer : IDecoderClient
    {
        private readonly string _decoderName;
        private readonly int _decoderPort;

        #region Private Properties

        private HttpPostServer _httpPostServer;

        #endregion //Private Properties

        public DecoderHttpServer(string _name, int _port)
        {
            _decoderName = _name;
            _decoderPort = _port;
        }

        #region Public Properties

        public bool IsRunning { get; private set; }

        #endregion //Public Properties

        #region Private Funtions

        private void HttpPostServer_MessageReceived(object sender, RestResponse e)
        {
            try
            {
                if (e == null)
                    return;

                var tetraControlResponse = JsonConvert.DeserializeObject<TetraControlResponse>(e.Content);
                if (tetraControlResponse == null || tetraControlResponse.data == null)
                    return;

                Logger.WriteDebug(
                    $"{_decoderName}: {tetraControlResponse.data.srcSSI} -> {tetraControlResponse.message}");
                switch (tetraControlResponse.data.type)
                {
                    case "status":
                        //Raise event
                        MessageReceived.RaiseEvent(this, new MessageEventArgs(tetraControlResponse.ToStatusString()));
                        break;
                    case "pos":
                        MessageReceived.RaiseEvent(this, new MessageEventArgs(tetraControlResponse.ToPositionString()));
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteDebug($"{_decoderName}: {ex} -> {ex.Message}");
            }
        }

        #endregion //Private Funtions


        public class HttpPostServer : HttpServer
        {
            public HttpPostServer(int port) : base(port)
            {
                Hostname = "+";

                Get["/"] = _ => $"RIS - TETRAControl Webserver running on Port {port}";

                Post["/"] = arg =>
                {
                    string documentContents;
                    using (var receiveStream = arg.InputStream)
                    {
                        using (var readStream = new StreamReader(receiveStream, Encoding.UTF8))
                        {
                            documentContents = readStream.ReadToEnd();
                        }
                    }

                    var response = new RestResponse
                    {
                        Content = documentContents
                    };
                    MessageReceived.Invoke(this, response);

                    return "OK";
                };
            }

            public event EventHandler<RestResponse> MessageReceived;
        }

        public class Data
        {
            public string type { get; set; }
            public DateTime ts { get; set; }
            public string destSSI { get; set; }
            public string destName { get; set; }
            public string srcSSI { get; set; }
            public string srcName { get; set; }


            public string status { get; set; }
            public string statusCode { get; set; }
            public string statusText { get; set; }
            public int radioID { get; set; }
            public string radioName { get; set; }
            public string remark { get; set; }


            public double Lat { get; set; }
            public double Lon { get; set; }
            public int Alt { get; set; }
            public int FixQual { get; set; }
        }

        public class TetraControlResponse
        {
            public string message { get; set; }
            public string sender { get; set; }
            public string type { get; set; }
            public DateTime timestamp { get; set; }
            public Data data { get; set; }


            public string ToStatusString()
            {
                return $"FMS\t{data.srcSSI}\t{data.status}";
            }

            public string ToPositionString()
            {
                return $"GPS\t{data.srcSSI}\t{data.Lat}\t{data.Lon}";
            }
        }

        #region Public Funtions

        public void Start()
        {
            try
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Starting");
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                IsRunning = false;

                if (_decoderPort <= 0 || _decoderPort >= 65535)
                {
                    Logger.WriteDebug($"{_decoderName}: Wrong setting -> decoderPort");
                    Disconnected.RaiseEvent(this, null);
                    return;
                }

                //Raise event
                Connecting.RaiseEvent(this, null);

                if (TryStartListener() == false)
                {
                    Logger.WriteDebug($"{_decoderName}: try to make url reservation");
                    NetAclChecker.AddAddress($"http://+:{_decoderPort}/");

                    if (TryStartListener() == false)
                    {
                        Logger.WriteDebug($"{_decoderName}: unable to start listener");
                        return;
                    }
                }

                //Raise event
                Connected.RaiseEvent(this, null);

                IsRunning = true;

                stopWatch.Stop();
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"Started -> {stopWatch.Elapsed}");
            }
            catch (Exception ex)
            {
                Logger.WriteDebug($"{_decoderName}: {ex} -> {ex.Message}");
            }
        }

        private bool TryStartListener()
        {
            try
            {
                _httpPostServer = new HttpPostServer(_decoderPort);
                _httpPostServer.MessageReceived += HttpPostServer_MessageReceived;
                _httpPostServer.Run();

                return true;
            }
            catch (HttpListenerException)
            {
                return false;
            }
        }

        public void Stop()
        {
            try
            {
                _httpPostServer?.Stop();

                //Raise event
                Disconnected.RaiseEvent(this, null);
            }
            catch (Exception ex)
            {
                Logger.WriteDebug($"{_decoderName}: {ex} -> {ex.Message}");
            }
        }

        public void EnableLoginString(string _loginString)
        {
            throw new NotImplementedException();
        }

        public void EnablePingTimer(string _pingString, double _pingInterval)
        {
            throw new NotImplementedException();
        }

        #endregion //Public Funtions

        #region Events

        public event EventHandler Connecting;
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler<MessageEventArgs> MessageReceived;

        #endregion //Events
    }
}