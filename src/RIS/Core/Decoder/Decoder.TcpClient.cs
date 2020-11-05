#region

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using SRS.Utilities;
using SRS.Utilities.Extensions;

#endregion

namespace RIS.Core.Decoder
{
    public class DecoderTcpClient : IDecoderClient
    {
        public enum ConnectionStatus
        {
            NeverConnected,
            Connecting,
            Connected,
            Reconnecting,
            Disconnecting,
            Disconnected,
            Stopped,

            ConnectError_Timeout,
            SendError_Timeout,
            Error
        }

        private readonly Encoding decoderEncoding;
        private readonly string decoderEtxString;
        private readonly string decoderName;
        private readonly int decoderPort;
        private readonly string decoderServer;

        public DecoderTcpClient(string _name, string _server, int _port, string _etxString, Encoding encoding)
        {
            decoderName = _name;
            decoderServer = _server;
            decoderPort = _port;
            decoderEtxString = _etxString;
            decoderEncoding = encoding;

            decoderStatus = ConnectionStatus.NeverConnected;
        }

        // State object for receiving data from remote device.
        public class StateObject
        {
            // Size of receive buffer.
            public const int BufferSize = 512;

            // Receive buffer.
            public byte[] Buffer = new byte[BufferSize];

            // Received data string.
            public string Data = string.Empty;

            // Client socket.
            public Socket Socket;
        }

        #region Public Properties

        #endregion //Public Properties

        #region Public Funtions

        public void Start()
        {
            try
            {
                if (!Regex.IsMatch(decoderServer,
                        @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$") &&
                    !Regex.IsMatch(decoderServer,
                        @"^(([a-zA-Z]|[a-zA-Z][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z]|[A-Za-z][A-Za-z0-9\-]*[A-Za-z0-9])$")
                )
                {
                    Logger.WriteDebug($"{decoderName}: Wrong setting -> decoderServer");
                    Disconnected.RaiseEvent(this, null);
                    return;
                }

                if (decoderPort <= 0 || decoderPort >= 65535)
                {
                    Logger.WriteDebug($"{decoderName}: Wrong setting -> decoderPort");
                    Disconnected.RaiseEvent(this, null);
                    return;
                }

                if (string.IsNullOrEmpty(decoderEtxString))
                {
                    Logger.WriteDebug($"{decoderName}: Wrong setting -> decoderEtxString");
                    Disconnected.RaiseEvent(this, null);
                    return;
                }

                Logger.WriteDebug($"{decoderName}: decoderEtxString -> {decoderEtxString}");

                //Timer for auto reconnect
                reconnectAttempts = 0;
                reconnectTimer = new Timer
                {
                    Interval = 1 * 60 * 1000, //1min reconnect
                    AutoReset = false
                };
                reconnectTimer.Elapsed += reconnectTimer_Elapsed;
                reconnectTimer.Stop();

                //Timeout for connect
                connectTimeout = new Timer
                {
                    Interval = TIMEOUT_INTERVAL,
                    AutoReset = false
                };
                connectTimeout.Elapsed += connectTimeout_Elapsed;
                connectTimeout.Stop();

                //Timeout for send
                sendTimeout = new Timer
                {
                    Interval = TIMEOUT_INTERVAL,
                    AutoReset = false
                };
                sendTimeout.Elapsed += sendTimeout_Elapsed;
                sendTimeout.Stop();

                //Timeout for receive
                receiveTimeout = new Timer
                {
                    Interval = 5 * 60 * 1000, //15min no data received
                    AutoReset = false
                };
                receiveTimeout.Elapsed += receiveTimeout_Elapsed;
                receiveTimeout.Stop();

                //Try to connect to TCP/IP-Server
                Connect();
            }
            catch (Exception ex)
            {
                Logger.WriteDebug($"{decoderName}: {ex} -> {ex.Message}");
            }
        }

        public void Stop()
        {
            try
            {
                Disconnect();

                pingTimer?.Stop();
                reconnectTimer?.Stop();
                connectTimeout?.Stop();
                sendTimeout?.Stop();

                //Set status
                decoderStatus = ConnectionStatus.Stopped;
            }
            catch (Exception ex)
            {
                Logger.WriteDebug($"{decoderName}: {ex} -> {ex.Message}");
            }
        }

        public void EnableLoginString(string _loginString)
        {
            try
            {
                Logger.WriteDebug($"{decoderName}: Enable login");

                loginString = _loginString;
            }
            catch (Exception ex)
            {
                Logger.WriteDebug($"{decoderName}: {ex} -> {ex.Message}");
            }
        }

        public void EnablePingTimer(string _pingString, double _pingInterval)
        {
            try
            {
                Logger.WriteDebug($"{decoderName}: Enable ping");

                if (string.IsNullOrEmpty(_pingString))
                {
                    Logger.WriteDebug($"{decoderName}: Wrong setting -> pingString");
                    return;
                }

                if (_pingInterval <= 0)
                {
                    Logger.WriteDebug($"{decoderName}: Wrong setting -> pingInterval");
                    return;
                }

                pingString = _pingString;
                //Timer to ping deocder
                pingTimer = new Timer
                {
                    Interval = _pingInterval,
                    AutoReset = false
                };
                pingTimer.Elapsed += pingTimer_Elapsed;
                pingTimer.Start();

                Logger.WriteDebug($"{decoderName}: pingTimer -> start");
            }
            catch (Exception ex)
            {
                Logger.WriteDebug($"{decoderName}: {ex} -> {ex.Message}");
            }
        }

        #endregion //Public Funtions

        #region Events

        public event EventHandler Connecting;
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler<MessageEventArgs> MessageReceived;

        #endregion //Events

        #region Private Properties

        private const int TIMEOUT_INTERVAL = 30000;

        private ConnectionStatus decoderStatus;
        private IPAddress decoderIpAddress;
        private Socket decoderSocket;
        private string loginString;

        private Timer pingTimer;
        private int pingCount;
        private string pingString;
        private Timer reconnectTimer;
        private int reconnectAttempts;
        private Timer connectTimeout;
        private Timer sendTimeout;
        private Timer receiveTimeout;

        #endregion //Private Properties

        #region Private Funtions

        private void pingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Restart timer
            pingTimer.Start();

            //Check Connection state
            if (decoderStatus != ConnectionStatus.Connected) return;

            //Send ping command
            Send(pingString + pingCount);

            //Reset every hour
            if (pingCount++ >= 60 * 60 / (pingTimer.Interval / 1000)) pingCount = 0;
        }

        /// <summary>
        ///     Try connecting to the remote host
        /// </summary>
        private void Connect()
        {
            Logger.WriteDebug($"{decoderName}: connect -> start");
            //Set status
            decoderStatus = ConnectionStatus.Connecting;
            //Raise event
            Connecting.RaiseEvent(this, null);

            try
            {
                //Release decoder
                if (decoderSocket != null)
                {
                    decoderSocket.Close();
                    decoderSocket = null;
                }

                //Parse server host/ip
                Logger.WriteDebug($"{decoderName}: connect -> server({decoderServer})");
                if (Regex.IsMatch(decoderServer,
                    @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$")
                )
                {
                    // the string is a IP   
                    decoderIpAddress = IPAddress.Parse(decoderServer);
                }
                else if (Regex.IsMatch(decoderServer,
                    @"^(([a-zA-Z]|[a-zA-Z][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z]|[A-Za-z][A-Za-z0-9\-]*[A-Za-z0-9])$"))
                {
                    // the string is a host -> try to resolve
                    var _ipHostInfo = Dns.GetHostEntry(decoderServer);
                    foreach (var ipAdress in _ipHostInfo.AddressList)
                        if (Regex.IsMatch(ipAdress.ToString(),
                            @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$")
                        )
                        {
                            decoderIpAddress = ipAdress;
                            break;
                        }

                    //If resolve fail -> abort
                    if (decoderIpAddress == null)
                    {
                        Logger.WriteDebug($"{decoderName}: cannot resolve hostname {decoderServer}");
                        Disconnected.RaiseEvent(this, null);
                        return;
                    }
                }

                Logger.WriteDebug($"{decoderName}: connect -> ip({decoderIpAddress})");

                //Create a TCP/IP socket 
                var _remoteEP = new IPEndPoint(decoderIpAddress, decoderPort);
                decoderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //Start timer for timeout
                connectTimeout.Start();
                //Connect to the remote endpoint
                decoderSocket.BeginConnect(_remoteEP, ConnectCallback, decoderSocket);
            }
            catch (SocketException ex)
            {
                Logger.WriteDebug($"{decoderName}: connect -> {ex.Message}");

                //Start reconnect
                Reconnect();
            }
            catch (Exception ex)
            {
                Logger.WriteDebug($"{decoderName}: {ex} -> {ex.Message}");
            }
        }

        /// <summary>
        ///     Tyr to disconnect and then connect again
        /// </summary>
        private void Reconnect()
        {
            Logger.WriteDebug($"{decoderName}: reconnect -> start");
            //Set status
            decoderStatus = ConnectionStatus.Reconnecting;
            //Raise event
            Connecting.RaiseEvent(this, null);
            //Stop timeouts
            receiveTimeout.Stop();
            sendTimeout.Stop();
            connectTimeout.Stop();

            if (reconnectAttempts == 0)
                //Start reconnect immediately
                reconnectTimer_Elapsed(this, null);
            else
                //Start reconnect after timer elapsed  
                reconnectTimer.Start();

            //Increase counter  
            reconnectAttempts++;
        }

        private void ConnectCallback(IAsyncResult _asyncResult)
        {
            Logger.WriteDebug($"{decoderName}: connect -> callback");

            //Retrieve the socket from the state object.
            var _socket = (Socket) _asyncResult.AsyncState;
            if (_socket == null) return;

            try
            {
                //Stop timeouts
                connectTimeout.Stop();
                receiveTimeout.Stop();
                sendTimeout.Stop();
                //Complete the connection.
                _socket.EndConnect(_asyncResult);
            }
            catch (Exception ex)
            {
                Logger.WriteDebug($"{decoderName}: connect -> {ex.Message}");
            }

            //Check Connection                                             
            if (_socket == null || !_socket.Connected)
            {
                Logger.WriteDebug($"{decoderName}: connect -> fail");
                //Set status
                decoderStatus = ConnectionStatus.Disconnected;
                //Raise event
                Disconnected.RaiseEvent(this, null);
                //Try to reconnect again;
                Reconnect();
            }
            else
            {
                Logger.WriteDebug($"{decoderName}: connect -> ok ({_socket.RemoteEndPoint})");
                //Set status
                decoderStatus = ConnectionStatus.Connected;
                //Raise event
                Connected.RaiseEvent(this, null);
                //Reset variables
                reconnectAttempts = 0;
                pingCount = 0;
                //Send LoginString
                Send(loginString);

                //Start receive timeout
                receiveTimeout.Start();
                //Begin receiving the data from the remote device.
                var _state = new StateObject();
                _state.Socket = _socket;
                _socket.BeginReceive(_state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, _state);
            }
        }

        private void connectTimeout_Elapsed(object sender, ElapsedEventArgs e)
        {
            Logger.WriteDebug($"{decoderName}: connect -> timeout elapsed");
            //Set status
            decoderStatus = ConnectionStatus.ConnectError_Timeout;
            //Raise event
            Disconnected.RaiseEvent(this, null);
            //Start reconnect
            Reconnect();
        }

        private void reconnectTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Logger.WriteDebug($"{decoderName}: reconnect -> attempt {reconnectAttempts}");
            //Set status
            decoderStatus = ConnectionStatus.Reconnecting;

            //Try to reconnect to server
            Connect();
        }

        /// <summary>
        ///     Try disconnecting from the remote host
        /// </summary>
        private void Disconnect()
        {
            Logger.WriteDebug($"{decoderName}: disconnect -> start");
            //Set status
            decoderStatus = ConnectionStatus.Disconnecting;

            //Stop timeouts
            connectTimeout.Stop();
            receiveTimeout.Stop();
            sendTimeout.Stop();

            try
            {
                if (decoderSocket != null && decoderSocket.Connected)
                {
                    decoderSocket.BeginDisconnect(false, DisconnectCallback, decoderSocket);
                }
                else
                {
                    Logger.WriteDebug($"{decoderName}: disconnect -> ok");
                    //Set status
                    decoderStatus = ConnectionStatus.Disconnected;
                    //Raise event
                    Disconnected.RaiseEvent(this, null);
                    //Release the socket.
                    decoderSocket.Close();
                }
            }
            catch (SocketException)
            {
            }
            catch (Exception ex)
            {
                Logger.WriteDebug($"{decoderName}: {ex} -> {ex.Message}");
            }
        }

        private void DisconnectCallback(IAsyncResult _asyncResult)
        {
            Logger.WriteDebug($"{decoderName}: disconnect -> callback");

            //Retrieve the socket from the state object.
            var _socket = (Socket) _asyncResult.AsyncState;
            if (_socket == null) return;

            try
            {
                //Complete disconnect
                _socket.EndDisconnect(_asyncResult);
                //Release the socket.
                _socket.Close();
            }
            catch (Exception ex)
            {
                Logger.WriteDebug($"{decoderName}: disconnect -> {ex.Message}");
            }
            finally
            {
                Logger.WriteDebug($"{decoderName}: disconnect -> ok");
                //Set status
                decoderStatus = ConnectionStatus.Disconnected;
                //Raise event
                Disconnected.RaiseEvent(this, null);
            }
        }

        private void ReceiveCallback(IAsyncResult _asyncResult)
        {
            try
            {
                //Stop receive timeout
                receiveTimeout.Stop();

                // Retrieve the state object and the client socket from the asynchronous state object.
                var _state = (StateObject) _asyncResult.AsyncState;
                if (_state == null || _state.Socket == null || decoderStatus == ConnectionStatus.Disconnected) return;

                var _socketError = new SocketError();

                //Complete recive
                var _bytesRead = _state.Socket.EndReceive(_asyncResult, out _socketError);
                if (_bytesRead > 0)
                {
                    //Store the data received so far
                    _state.Data += decoderEncoding.GetString(_state.Buffer, 0, _bytesRead);
                    //Logger.WriteDebug($"{decoderName}: receive -> {_state.Data}");

                    //Split each frame
                    var _dataArray =
                        _state.Data.Split(new[] {decoderEtxString}, StringSplitOptions.RemoveEmptyEntries);
                    if (_dataArray != null && _dataArray.Count() > 0)
                        //Loop through all frames
                        foreach (var _data in _dataArray)
                        {
                            //Raise event
                            MessageReceived.RaiseEvent(this, new MessageEventArgs(_data));
                            //Clear buffer
                            _state.Data = _state.Data.Replace(_data + decoderEtxString, string.Empty);
                        }


                    if (_state.Socket != null && _state.Socket.Connected)
                    {
                        //Start receive timeout again
                        receiveTimeout.Start();
                        //Start receive again
                        _state.Socket.BeginReceive(_state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback,
                            _state);
                    }
                    else
                    {
                        //Raise event
                        Disconnected.RaiseEvent(this, null);
                        //Try to reconnect again;
                        Reconnect();
                    }
                }
                //EndRecieve returns 0 bytes -> connection have been closed by the remote endpoint.
                else if (_bytesRead == 0 && decoderStatus == ConnectionStatus.Connected)
                {
                    Logger.WriteDebug($"{decoderName}: receive -> connection closed from server");

                    //Raise event
                    Disconnected.RaiseEvent(this, null);
                    //Try to reconnect again;
                    Reconnect();
                }
                else if (_socketError != SocketError.Success)
                {
                    Logger.WriteDebug($"{decoderName}: receive -> SocketError({_socketError})");

                    //Raise event
                    Disconnected.RaiseEvent(this, null);
                    //Try to reconnect again;
                    Reconnect();
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                Logger.WriteDebug($"{decoderName}: {ex} -> {ex.Message}");
            }
        }

        private void receiveTimeout_Elapsed(object sender, ElapsedEventArgs e)
        {
            Logger.WriteDebug($"{decoderName}: receive -> timeout elapsed");

            //Start receive timeout again
            receiveTimeout.Start();
            //Send something to check connection
            Send("!");
        }

        private void Send(string _message)
        {
            try
            {
                //Stop timer for timeout
                sendTimeout.Stop();

                if (decoderSocket == null || !decoderSocket.Connected || string.IsNullOrEmpty(_message)) return;

                //Convert the string data to byte data using ASCII encoding.
                var _data = Encoding.ASCII.GetBytes(_message + decoderEtxString);

                //Start timer for timeout
                sendTimeout.Start();
                //Begin sending the data to the remote device.
                decoderSocket.BeginSend(_data, 0, _data.Length, 0, SendCallback, decoderSocket);

                Logger.WriteDebug($"{decoderName}: send -> Message({_message})");
            }
            catch (Exception ex)
            {
                Logger.WriteDebug($"{decoderName}: {ex} -> {ex.Message}");
            }
        }

        private void SendCallback(IAsyncResult _asyncResult)
        {
            //Stop timeout timer
            sendTimeout.Stop();

            // Retrieve the socket from the state object.
            var _socket = (Socket) _asyncResult.AsyncState;
            if (_socket == null) return;

            try
            {
                var _socketError = new SocketError();

                // Complete sending the data to the remote device.
                var _bytesSent = _socket.EndSend(_asyncResult, out _socketError);
                if (_bytesSent > 0)
                {
                    Logger.WriteDebug($"{decoderName}: send -> {_bytesSent} bytes");
                }
                //EndRecieve returns 0 bytes -> connection have been closed by the remote endpoint.
                else if (_bytesSent == 0 && decoderStatus == ConnectionStatus.Connected)
                {
                    Logger.WriteDebug($"{decoderName}: send -> connection closed from server");

                    //Raise event
                    Disconnected.RaiseEvent(this, null);
                    //Try to reconnect again;
                    Reconnect();
                }
                else if (_socketError != SocketError.Success)
                {
                    Logger.WriteDebug($"{decoderName}: send -> SocketError({_socketError})");

                    //Raise event
                    Disconnected.RaiseEvent(this, null);
                    //Try to reconnect again;
                    Reconnect();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteDebug($"{decoderName}: {ex} -> {ex.Message}");
            }
        }

        private void sendTimeout_Elapsed(object sender, ElapsedEventArgs e)
        {
            Logger.WriteDebug($"{decoderName}: send -> timeout elapsed");
            //Set status
            decoderStatus = ConnectionStatus.SendError_Timeout;

            //Raise event
            Disconnected.RaiseEvent(this, null);
            //Start reconnect
            Reconnect();
        }

        #endregion //Private Funtions
    }
}