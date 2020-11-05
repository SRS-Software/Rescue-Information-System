#region

using System;
using RIS.Core.Riverlevel;

#endregion

namespace RIS.Core
{
    public interface IRiverlevelService : IService
    {
        #region Properties

        #endregion //Properties

        #region Functions

        #endregion //Functions

        #region Events

        /// <summary>
        ///     Event is raised if new data received
        /// </summary>
        event EventHandler<DataReceivedEventArgs> DataReceived;

        /// <summary>
        ///     Event is raised if new data received
        /// </summary>
        event EventHandler<byte[]> ImageReceived;

        #endregion //Events
    }
}