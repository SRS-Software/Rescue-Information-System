#region

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Timers;
using HtmlAgilityPack;
using RIS.Properties;
using SRS.Utilities;
using SRS.Utilities.Extensions;
using DataReceivedEventArgs = RIS.Core.Riverlevel.DataReceivedEventArgs;

#endregion

namespace RIS.Core
{
    public class RiverlevelService : IRiverlevelService
    {
        private readonly Timer _updateTimer;

        public RiverlevelService()
        {
            try
            {
                _updateTimer = new Timer();
                _updateTimer.AutoReset = false;
                _updateTimer.Interval = 60000 * 10; //10min
                _updateTimer.Elapsed += updateTimer_Elapsed;

                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Initialize");
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #region Public Properties

        public bool IsRunning { get; private set; }

        #endregion //Public Properties

        #region Public Funtions

        public void Start()
        {
            try
            {
                if (string.IsNullOrEmpty(Settings.Default.Riverlevel_Messstelle))
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Riverlevel disabled in settings");
                    return;
                }

                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Starting");
                var _stopWatch = new Stopwatch();
                _stopWatch.Start();

                IsRunning = false;

                //Query first time
                updateTimer_Elapsed(this, null);

                IsRunning = true;

                _stopWatch.Stop();
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"Started -> {_stopWatch.Elapsed}");
            }
            catch (Exception ex)
            {
                ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs
                {
                    Methode = MethodBase.GetCurrentMethod(),
                    Error = ex
                });
            }
        }

        public void Stop()
        {
            try
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Stopping");
                var _stopWatch = new Stopwatch();
                _stopWatch.Start();

                _updateTimer.Stop();

                _stopWatch.Stop();
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"Stopped -> {_stopWatch.Elapsed}");
            }
            catch (Exception ex)
            {
                ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs
                {
                    Methode = MethodBase.GetCurrentMethod(),
                    Error = ex
                });
            }
            finally
            {
                IsRunning = false;
            }
        }

        #endregion //Public Funtions

        #region Events

        public event EventHandler<ExceptionEventArgs> ExceptionOccured;
        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public event EventHandler<byte[]> ImageReceived;

        #endregion //Events

        #region Private Properties

        #endregion //Private Properties

        #region Private Funtions

        private void updateTimer_Elapsed(object source, ElapsedEventArgs e)
        {
            try
            {
                //Set URL to query
                string _queryUrl = null;
                switch (Settings.Default.Riverlevel_Mode)
                {
                    case RiverlevelMode.HndBayern:
                        _queryUrl = "http://www.hnd.bayern.de/pegel";
                        break;
                    default:
                        return;
                }

                //Query riverlevel
                using (var _client = new WebClient())
                {
                    _client.DownloadStringCompleted += client_DownloadDataCompleted;
                    _client.DownloadStringAsync(new Uri(_queryUrl));
                }
            }
            catch (Exception ex)
            {
                ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs
                {
                    Methode = MethodBase.GetCurrentMethod(),
                    Error = ex
                });
            }
            finally
            {
                //Restart timer
                _updateTimer.Start();
            }
        }

        private void client_DownloadDataCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    Logger.WriteError(MethodBase.GetCurrentMethod(), e.Error.Message);
                    return;
                }

                switch (Settings.Default.Riverlevel_Mode)
                {
                    case RiverlevelMode.HndBayern:
                        parseHndBayern(e.Result);
                        break;
                }
            }
            catch (Exception ex)
            {
                ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs
                {
                    Methode = MethodBase.GetCurrentMethod(),
                    Error = ex
                });
            }
        }

        private void parseHndBayern(string result)
        {
            if (string.IsNullOrWhiteSpace(result)) return;

            //Load Html Document
            var _document = new HtmlDocument();
            _document.LoadHtml(result);

            //Get Img-Element with id
            var _imgElement = _document.DocumentNode.Descendants().Where(x =>
                x.Name == "img" && x.Attributes["id"] != null &&
                x.Attributes["id"].Value.Contains(Settings.Default.Riverlevel_Messstelle)).FirstOrDefault();
            if (_imgElement == null) return;

            //Create EventArg
            var _arg = new DataReceivedEventArgs
            {
                Description = _imgElement.GetAttributeValue("data-zeile2", "").Trim() + " - " +
                              _imgElement.GetAttributeValue("data-name", "").Trim(),
                Riverlevel_Description = "Wasserstand:",
                Riverlevel_Value = _imgElement.GetAttributeValue("data-wert", "").Trim() + " cm",
                Flowspeed_Description = "Abfluss:",
                Flowspeed_Value = _imgElement.GetAttributeValue("data-wert2", "").Trim() + " m³/s",
                Warning = _imgElement.GetAttributeValue("data-ms", "").Trim(),
                DataDate = _imgElement.GetAttributeValue("data-datum", "").Trim()
            };
            DataReceived.RaiseEvent(this, _arg);


            //Query riverlevel
            var _imageUrl = "http://www.hnd.bayern.de/webservices/minigraphik.php?days=7&roh=0&ms=get&cache=hnd";
            _imageUrl += $"&{_imgElement.GetAttributeValue("data-graph", "").Trim()}";
            _imageUrl += $"&breite={Settings.Default.Riverlevel_ImageSize.X}";
            _imageUrl += $"&hoehe={Settings.Default.Riverlevel_ImageSize.Y}";
            using (var _client = new WebClient())
            {
                _client.DownloadDataCompleted += client_DownloadImageCompleted;
                _client.DownloadDataAsync(new Uri(_imageUrl));
            }
        }

        private void client_DownloadImageCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    Logger.WriteError(MethodBase.GetCurrentMethod(), e.Error.Message);
                    return;
                }

                if (e.Result != null) ImageReceived.Invoke(this, e.Result);
            }
            catch (Exception ex)
            {
                ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs
                {
                    Methode = MethodBase.GetCurrentMethod(),
                    Error = ex
                });
            }
        }

        #endregion //Private Funtions
    }
}