#region

using System;
using RIS.Core.Ams;

#endregion

namespace RIS.Core
{
    public interface IAmsService : IService
    {
        #region Events

        /// <summary>
        /// </summary>
        event EventHandler<EinsatzFinishedEventArgs> EinsatzFinished;

        #endregion //Events

        #region Properties

        #endregion //Properties

        #region Functions

        #endregion //Functions
    }
}