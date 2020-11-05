#region

using System;

#endregion

namespace RIS.Core
{
    public interface IWarnweatherService : IService
    {
        #region Events

        /// <summary>
        ///     Event is raised if new data received
        /// </summary>
        event EventHandler<byte[]> ImageReceived;

        #endregion //Events

        #region Properties

        #endregion //Properties

        #region Functions

        #endregion //Functions
    }
}