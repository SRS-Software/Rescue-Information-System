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
    public class DecoderTcpServer : IDecoderClient
    {
        public enum ConnectionStatus
        {
            NeverConnected,
            Listening,
            Restart,
            ClientConnected,
            Disconnecting,
            ClientDisconnected,

            SendFail_Timeout,
            Error
        }

        private readonly string decoderEtxString;

        private readonly string decoderName;
        private readonly int decoderPort;
        private readonly string decoderServer;

        public DecoderTcpServer(string _name, string _server, int _port, string _etxString)
        {
            decoderName = _name;
            decoderServer = _server;
            decoderPort = _port;
            decoderEtxString = _etxString;

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
        }

        #region Public Properties

        #endregion //Public Properties

        #region Public Funtions

        public void Start()
        {
            try
            {
                if (Regex.IsMatch(decoderServer,
                    @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$")
                )
                {
                    // the string is an IP
                    decoderIpAddress = IPAddress.Parse(decoderServer);
                }
                else if (Regex.IsMatch(decoderServer,
                    @"^(([a-zA-Z]|[a-zA-Z][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z]|[A-Za-z][A-Za-z0-9\-]*[A-Za-z0-9])$"))
                {
                    // the string is a host   
                    var _ipHostInfo = Dns.GetHostEntry(decoderServer);
                    decoderIpAddress = _ipHostInfo.AddressList[0];
                }
                else
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

                //Timer for auto reconnect
                restartAttempts = 0;
                restartTimer = new Timer
                {
                    Interval = 1000, //will be increased in handler
                    AutoReset = false
                };
                restartTimer.Elapsed += reconnectTimer_Elapsed;
                restartTimer.Stop();

                //Timeout for receive
                sendTimeout = new Timer
                {
                    Interval = TIMEOUT_INTERVAL,
                    AutoReset = false
                };
                sendTimeout.Elapsed += sendTimeout_Elapsed;
                sendTimeout.Stop();

                //Connect to TCP/IP-Server
                StartListening();
            }
            catch (Exception ex)
            {
                Logger.WriteDebug($"{decoderName}: {ex} -> {ex.Message}");
            }
        }

        public void Stop()
        {
            Disconnect();

            //Stop all timer 
            if (pingTimer != null) pingTimer.Stop();

            if (restartTimer != null) restartTimer.Stop();

            if (sendTimeout != null) sendTimeout.Stop();
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
                Logger.WriteDebug($"{decoderName}: Start ping timer");

                pingString = _pingString;

                //Timer to ping deocder
                pingTimer = new Timer
                {
                    Interval = _pingInterval,
                    AutoReset = false
                };
                pingTimer.Elapsed += pingTimer_Elapsed;
                pingTimer.Start();
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

        private const int TIMEOUT_INTERVAL = 5000;

        private ConnectionStatus decoderStatus;
        private IPAddress decoderIpAddress;
        private Socket decoderSocket;
        private Socket listenerSocket;
        private string loginString;

        private Timer pingTimer;
        private int pingAttempts;
        private string pingString;
        private Timer restartTimer;
        private int restartAttempts;
        private Timer sendTimeout;

        #endregion //Private Properties

        #region Private Funtions

        private void pingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Send(pingString + pingAttempts);

            if (pingTimer != null)
            {
                //Reset every hour
                if (pingAttempts++ >= 60 * 60 / (pingTimer.Interval / 1000)) pingAttempts = 0;

                pingTimer.Start();
            }
        }

        private void reconnectTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (restartAttempts++ >= 10 || restartTimer == null)
            {
                Logger.WriteDebug($"{decoderName}: Auto-Restart failed after 10 attempts");
                Stop();
            }
            else
            {
                Logger.WriteDebug($"{decoderName}: Auto-Restart attempt {restartAttempts}");

                if (decoderSocket != null && decoderSocket.Connected)
                {
                    //Disconnect and then connect in Callback again 
                    decoderSocket.Shutdown(SocketShutdown.Both);
                    decoderSocket.BeginDisconnect(true, DisconnectByServerCallback, decoderSocket);
                }
                else
                {
                    //Try to connect to server
                    StartListening();
                }
            }
        }

        private void sendTimeout_Elapsed(object sender, ElapsedEventArgs e)
        {
            decoderStatus = ConnectionStatus.SendFail_Timeout;

            //Raise event
            Disconnected.RaiseEvent(this, null);
            Restart();
        }

        private void StartListening()
        {
            if (decoderStatus == ConnectionStatus.Listening ||
                decoderStatus == ConnectionStatus.ClientConnected) return;

            Logger.WriteDebug($"{decoderName}: Start to listening");
            decoderStatus = ConnectionStatus.Listening;

            //Raise event
            Connecting.RaiseEvent(this, null);

            // Bind the socket to the local endpoint and listen for incoming connections
            var _localEP = new IPEndPoint(decoderIpAddress, decoderPort);
            listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listenerSocket.Bind(_localEP);
                listenerSocket.Listen(1);

                // Start an asynchronous socket to listen for connections.
                Logger.WriteDebug($"{decoderName}: Waiting for client");
                listenerSocket.BeginAccept(AcceptCallback, listenerSocket);
            }
            catch (Exception ex)
            {
                Logger.WriteDebug($"{decoderName}: {ex} -> {ex.Message}");
            }
        }

        public void AcceptCallback(IAsyncResult _asyncResult)
        {
            //Get the socket that handles the client request.
            var _listenerSocket = (Socket) _asyncResult.AsyncState;
            if (_listenerSocket == null) return;

            //Get clientSocket
            try
            {
                decoderSocket = _listenerSocket.EndAccept(_asyncResult);

                //Shutdown listener only one client allowed
                listenerSocket.Close();
                listenerSocket = null;
            }
            catch (Exception ex)
            {
                Logger.WriteDebug($"{decoderName}: {ex} -> {ex.Message}");
            }

            //Check Connection
            if (decoderSocket == null || !decoderSocket.Connected)
            {
                Logger.WriteDebug($"{decoderName}: Disconnected");
                decoderStatus = ConnectionStatus.ClientDisconnected;

                //Raise event
                Disconnected.RaiseEvent(this, null);
                //Try to reconnect again;
                Restart();

                return;
            }


            //Start to receive
            try
            {
                Logger.WriteDebug($"{decoderName}: Connection from {decoderSocket.RemoteEndPoint}");
                decoderStatus = ConnectionStatus.ClientConnected;

                restartAttempts = 0;
                pingAttempts = 0;

                //Send LoginString
                Send(loginString);

                // Begin receiving the data from the remote device.
                var _state = new StateObject();
                decoderSocket.BeginReceive(_state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, _state);

                //Raise event
                Connected.RaiseEvent(this, null);
            }
            catch (Exception ex)
            {
                Logger.WriteDebug($"{decoderName}: {ex} -> {ex.Message}");
            }
        }

        /// <summary>
        ///     Tyr to disconnect and then connect again
        /// </summary>
        private void Restart()
        {
            Logger.WriteDebug($"{decoderName}: Auto-Restart start");
            decoderStatus = ConnectionStatus.Restart;

            //Raise event
            Connecting.RaiseEvent(this, null);

            //Start reconnect after timer elapsed   
            restartTimer.Interval = restartAttempts == 0 ? 1000 : restartAttempts * 60000;
            restartTimer.Start();
        }

        /// <summary>
        ///     Try disconnecting from the remote host
        /// </summary>
        private void Disconnect()
        {
            Logger.WriteDebug($"{decoderName}: Start to disconnect");
            decoderStatus = ConnectionStatus.Disconnecting;

            //Shutdown listener
            if (listenerSocket != null)
            {
                listenerSocket.Close();
                listenerSocket = null;
            }

            try
            {
                //Shutdown socket
                decoderSocket.Shutdown(SocketShutdown.Both);
                decoderSocket.BeginDisconnect(true, DisconnectCallback, decoderSocket);
            }
            catch (Exception)
            {
                Logger.WriteDebug($"{decoderName}: Disconnected");
                decoderStatus = ConnectionStatus.ClientDisconnected;

                // Release the socket.
                if (decoderSocket != null) decoderSocket.Close();

                //Raise event
                Disconnected.RaiseEvent(this, null);
            }
        }

        private void DisconnectCallback(IAsyncResult _asyncResult)
        {
            // Retrieve the socket from the state object.
            var _socket = (Socket) _asyncResult.AsyncState;
            if (_socket == null) return;

            Logger.WriteDebug($"{decoderName}: Disconnected");
            decoderStatus = ConnectionStatus.ClientDisconnected;

            // Complete the connection.
            _socket.EndDisconnect(_asyncResult);

            // Release the socket.
            decoderSocket.Close();

            //Raise event
            Disconnected.RaiseEvent(this, null);
        }

        private void DisconnectByServerCallback(IAsyncResult _asyncResult)
        {
            //Retrieve the socket from the state object.
            var _socket = (Socket) _asyncResult.AsyncState;
            if (_socket == null) return;

            Logger.WriteDebug($"{decoderName}: Disconnected");
            decoderStatus = ConnectionStatus.ClientDisconnected;

            //Complete the connection.
            _socket.EndDisconnect(_asyncResult);

            // Release the socket.
            decoderSocket.Close();

            //Raise event
            Disconnected.RaiseEvent(this, null);

            //Try to start server
            StartListening();
        }

        public void ReceiveCallback(IAsyncResult _asyncResult)
        {
            // Retrieve the state object and the handler socket from the asynchronous state object.
            var _state = (StateObject) _asyncResult.AsyncState;
            if (_state == null || decoderSocket == null) return;

            var _socketError = new SocketError();
            var _bytesRead = 0;
            try
            {
                // Read data from the remote device.
                _bytesRead = decoderSocket.EndReceive(_asyncResult, out _socketError);

                //EndRecieve returns x bytes -> connection ok
                if (_bytesRead > 0)
                {
                    //Store the data received so far
                    _state.Data += Encoding.ASCII.GetString(_state.Buffer, 0, _bytesRead);

                    //Split each frame
                    var _dataArray = _state.Data.Split(new[]
                    {
                        decoderEtxString
                    }, StringSplitOptions.RemoveEmptyEntries);
                    if (_dataArray != null && _dataArray.Count() > 0)
                        //Loop through all frames
                        foreach (var _data in _dataArray)
                        {
                            Logger.WriteDebug($"{decoderName}: Message -> {_data}");

                            //Remove frame + etx from buffer
                            _state.Data = _state.Data.Replace(_data + decoderEtxString, "");
                            //Raise event
                            MessageReceived.RaiseEvent(this, new MessageEventArgs(_data));
                        }

                    if (decoderSocket != null && decoderSocket.Connected)
                        //Start receive again  
                        decoderSocket.BeginReceive(_state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback,
                            _state);
                }
                //EndRecieve returns 0 bytes -> connection have been closed by the remote endpoint. 
                else if (_bytesRead == 0 && decoderStatus == ConnectionStatus.ClientConnected)
                {
                    Logger.WriteDebug($"{decoderName}: Connection closed from client");

                    //Raise event
                    Disconnected.RaiseEvent(this, null);
                    //Try to reconnect again;
                    Restart();
                }
                else if (_socketError != SocketError.Success)
                {
                    Logger.WriteDebug($"{decoderName}: SocketError -> {_socketError}");
                }
            }
            catch (Exception ex)
            {
                Logger.WriteDebug($"{decoderName}: Error on receive -> {ex.Message}");
                //Raise event
                Disconnected.RaiseEvent(this, null);
                //Try to reconnect again;
                Restart();
            }
        }

        private void Send(string _message)
        {
            if (decoderStatus != ConnectionStatus.ClientConnected || decoderSocket == null ||
                !decoderSocket.Connected || string.IsNullOrEmpty(_message))
                return;

            // Convert the string data to byte data using ASCII encoding.
            var byteData = Encoding.ASCII.GetBytes(_message + decoderEtxString);

            // Begin sending the data to the remote device.
            decoderSocket.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, decoderSocket);

            //start timer for timeout
            sendTimeout.Start();
        }

        private void SendCallback(IAsyncResult _asyncResult)
        {
            // Retrieve the socket from the state object.
            var _socket = (Socket) _asyncResult.AsyncState;
            if (_socket == null) return;

            //Stop timeout timer
            sendTimeout.Stop();

            var _socketError = new SocketError();
            var _bytesSent = 0;
            try
            {
                // Complete sending the data to the remote device.
                _bytesSent = _socket.EndSend(_asyncResult, out _socketError);
            }
            catch (Exception ex)
            {
                Logger.WriteDebug($"{decoderName}: {ex} -> {ex.Message}");
            }

            if (_bytesSent > 0)
            {
                Logger.WriteDebug($"{decoderName}: Message with {_bytesSent} bytes sent");
            }
            //EndRecieve returns 0 bytes -> connection have been closed by the remote endpoint.
            else if (_bytesSent == 0 && decoderStatus == ConnectionStatus.ClientConnected)
            {
                Logger.WriteDebug($"{decoderName}: Connection closed from server");

                //Raise event
                Disconnected.RaiseEvent(this, null);
                //Try to reconnect again;
                Restart();
            }
            else if (_socketError != SocketError.Success)
            {
                Logger.WriteDebug($"{decoderName}: SocketError -> {_socketError}");
            }
        }

        #endregion //Private Funtions
    }
}