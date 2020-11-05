#region

using System;

#endregion

namespace RIS.Core
{
    public interface IService
    {
        #region Properties

        bool IsRunning { get; }

        #endregion //Properties

        #region Events

        /// <summary>
        ///     Exception occured in service.
        ///     The Service should be restarted
        /// </summary>
        event EventHandler<ExceptionEventArgs> ExceptionOccured;

        #endregion //Events

        #region Functions

        void Start();
        void Stop();

        #endregion //Functions
    }
}