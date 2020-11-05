#region

using System;
using RIS.Core.Map;

#endregion

namespace RIS.Core
{
    public interface IMapService : IService
    {
        #region Functions

        string Geocode(string _address);

        #endregion //Functions

        #region Events

        /// <summary>
        /// </summary>
        event EventHandler<FinishedEventArgs> Finished;

        #endregion //Events

        #region Properties

        #endregion //Properties
    }
}