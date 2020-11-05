#region

using System;
using RIS.Core.Decoder;

#endregion

namespace RIS.Core
{
    public interface IDecoderService : IService
    {
        #region Properties

        #endregion //Properties

        #region Functions

        #endregion //Functions

        #region Events

        /// <summary>
        /// </summary>
        event EventHandler<StatusEventArgs> StatusChanged;

        /// <summary>
        /// </summary>
        event EventHandler<FmsMessageEventArgs> FmsMessageReceived;

        /// <summary>
        /// </summary>
        event EventHandler<PagerMessageEventArgs> PagerMessageReceived;

        #endregion //Events
    }
}