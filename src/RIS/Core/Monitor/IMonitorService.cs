#region

using RIS.Views;

#endregion

namespace RIS.Core
{
    public interface IMonitorService : IService
    {
        #region Properties

        #endregion //Properties

        #region Functions

        void AddAlarmWindow(AlarmWindow _alarmWindow);
        void RemoveAlarmWindow(AlarmWindow _alarmWindow);
        bool IsAlarmWindow();

        #endregion //Functions

        #region Events

        #endregion //Events
    }
}