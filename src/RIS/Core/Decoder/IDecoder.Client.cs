#region

using System;

#endregion

namespace RIS.Core.Decoder
{
    public interface IDecoderClient
    {
        #region Properties

        #endregion //Properties

        #region Functions

        void Start();
        void Stop();
        void EnableLoginString(string _loginString);
        void EnablePingTimer(string _pingString, double _pingInterval);

        #endregion //Functions

        #region Events

        /// <summary>
        ///     Raised when the DecoderClient start to connect
        /// </summary>
        event EventHandler Connecting;

        /// <summary>
        ///     Raised when the DecoderClient is connected
        /// </summary>
        event EventHandler Connected;

        /// <summary>
        ///     Raised when the DecoderClient is disconnected
        /// </summary>
        event EventHandler Disconnected;

        /// <summary>
        ///     Raised when the DecoderClient received a new message
        /// </summary>
        event EventHandler<MessageEventArgs> MessageReceived;

        #endregion //Events
    }
}