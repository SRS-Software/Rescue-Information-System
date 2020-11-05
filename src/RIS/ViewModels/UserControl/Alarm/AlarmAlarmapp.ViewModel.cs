#region

using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Timers;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using RIS.Core;
using RIS.Core.Alarmapp;
using RIS.Core.Fax;
using SRS.Utilities;

#endregion

namespace RIS.ViewModels
{
    public class AlarmAlarmappViewModel : ViewModelBase
    {
        private readonly IAlarmappService alarmappService;
        private readonly string einsatzGuid;

        public AlarmAlarmappViewModel(Einsatz _einsatz)
        {
            try
            {
                einsatzGuid = _einsatz.Guid;

                alarmappService = ServiceLocator.Current.GetInstance<IAlarmappService>();
                if (alarmappService != null)
                    alarmappService.Alarmed += (sender, e) => DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        alarmappService_Alarmed(sender, e);
                    });
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public override void Cleanup()
        {
            if (updateTimer != null) updateTimer.Stop();

            if (alarmappService != null)
                alarmappService.Alarmed -= (sender, e) => DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    alarmappService_Alarmed(sender, e);
                });

            base.Cleanup();
        }

        #region Commands

        #endregion //Commands

        #region Events

        #endregion //Events

        #region Validation

        #endregion //Validation

        #region Public Properties

        private int alarmedUser;

        public int AlarmedUser
        {
            get => alarmedUser;
            set
            {
                if (alarmedUser == value) return;

                alarmedUser = value;

                RaisePropertyChanged(() => AlarmedUser);
            }
        }

        private int accpetedUser;

        public int AccpetedUser
        {
            get => accpetedUser;
            set
            {
                if (accpetedUser == value) return;

                accpetedUser = value;

                RaisePropertyChanged(() => AccpetedUser);
            }
        }

        private int rejectedUser;

        public int RejectedUser
        {
            get => rejectedUser;
            set
            {
                if (rejectedUser == value) return;

                rejectedUser = value;

                RaisePropertyChanged(() => RejectedUser);
            }
        }

        public ObservableCollection<AlarmappOverviewGroupViewModel> GroupList { get; } =
            new ObservableCollection<AlarmappOverviewGroupViewModel>();

        private int userRowCurrent;
        private int userRows;

        public int UserRows
        {
            get => userRows;
            set
            {
                if (userRows == value) return;

                userRows = value;

                RaisePropertyChanged(() => UserRows);
            }
        }

        private int userColumns;

        public int UserColumns
        {
            get => userColumns;
            set
            {
                if (userColumns == value)
                    return;
                userColumns = value;
                RaisePropertyChanged(() => UserColumns);
            }
        }

        public ObservableCollection<AlarmappUserViewModel> UserList { get; } =
            new ObservableCollection<AlarmappUserViewModel>();

        #endregion //Public Properties

        #region Public Functions

        #endregion //Public Functions

        #region Private Properties

        private Timer updateTimer { get; set; }
        private string operationId { get; set; }

        #endregion //Private Properties

        #region Private Functions

        private void alarmappService_Alarmed(object sender, AlarmedEventArgs e)
        {
            if (e == null || e.EinsatzGuid != einsatzGuid) return;

            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                operationId = e.OperationId;
                if (string.IsNullOrEmpty(operationId)) return;

                //Init timer to reset alarmierungen
                updateTimer = new Timer();
                updateTimer.Interval = 2500;
                updateTimer.Elapsed += updateTimer_Elapsed;
                updateTimer.AutoReset = false;

                //Execute handler
                updateTimer_Elapsed(this, null);
            });
        }

        private void updateTimer_Elapsed(object source, ElapsedEventArgs e)
        {
            try
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    var _alarmstatus = alarmappService.GetAlarmStatus(operationId);
                    if (_alarmstatus == null)
                        return;

                    GroupList.Clear();
                    UserList.Clear();
                    UserColumns = 1;
                    UserRows = 0;
                    userRowCurrent = 0;

                    AlarmedUser = _alarmstatus.AlarmedUser;
                    AccpetedUser = _alarmstatus.AccpetedUser;
                    RejectedUser = _alarmstatus.RejectedUser;

                    #region OverviewView

                    foreach (var functiongroup in _alarmstatus.Functiongroups)
                    {
                        var overviewGroupViewModel = new AlarmappOverviewGroupViewModel(GroupList.Count,
                            functiongroup.Id, functiongroup.Name, functiongroup.UserCount, functiongroup.Foreground,
                            functiongroup.Background);
                        GroupList.Add(overviewGroupViewModel);
                        RaisePropertyChanged(() => GroupList);
                    }

                    #endregion //OverviewView

                    #region UserView

                    foreach (var user in _alarmstatus.Users)
                    {
                        var userViewModel = new AlarmappUserViewModel(userRowCurrent, UserColumns - 1, user.Id,
                            user.Name, user.StatusColor);
                        UserList.Add(userViewModel);
                        RaisePropertyChanged(() => UserList);

                        userRowCurrent++;
                        if (userRowCurrent >= 15)
                        {
                            userRowCurrent = 0;
                            UserColumns++;
                        }

                        if (UserRows < 15) UserRows++;

                        //Update User Status and Groups
                        userViewModel.ClearFunctiongroups();
                        foreach (var functiongroup in user.Functiongroups)
                            userViewModel.AddFunctiongroups(functiongroup.Code, functiongroup.Foreground,
                                functiongroup.Background);
                    }

                    #endregion //UserView
                });
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
            finally
            {
                //Start update timer again
                updateTimer.Start();
            }
        }

        #endregion //Private Funtions
    }
}