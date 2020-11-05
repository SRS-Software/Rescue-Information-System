#region

using System;
using RIS.Core.Alarmapp;

#endregion

namespace RIS.Core
{
    public interface IAlarmappService : IService
    {
        #region Events

        /// <summary>
        /// </summary>
        event EventHandler<AlarmedEventArgs> Alarmed;

        /// <summary>
        /// </summary>
        //event EventHandler<UpdatedEventArgs> Updated;

        #endregion //Events

        #region Properties

        #endregion //Properties

        #region Functions

        bool ClearAlarmgroups();

        bool Refresh(string apiToken, string organisationId);

        AlarmStatus GetAlarmStatus(string operationIds);

        #endregion //Functions
    }
}