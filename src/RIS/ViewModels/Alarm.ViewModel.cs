#region

using System;
using System.Drawing;
using System.Reflection;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using RIS.Business;
using RIS.Core.Fax;
using RIS.Properties;
using SRS.Utilities;

#endregion

namespace RIS.ViewModels
{
    public sealed class AlarmViewModel : ViewModelBase
    {
        private readonly IBusiness _business;
        private readonly string _einsatzGuid;

        public AlarmViewModel(Einsatz einsatz)
        {
            try
            {
                _einsatzGuid = einsatz.Guid;

                _business = ServiceLocator.Current.GetInstance<IBusiness>();

                AlarmappVM = new AlarmAlarmappViewModel(einsatz);
                DataVM = new AlarmDataViewModel(einsatz);
                RouteVM = new AlarmRouteViewModel(einsatz);
                TimerVM = new AlarmTimerViewModel(einsatz);
                VehiclesVM = new AlarmVehiclesViewModel(einsatz);
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public override void Cleanup()
        {
            AlarmappVM.Cleanup();
            DataVM.Cleanup();
            RouteVM.Cleanup();
            TimerVM.Cleanup();
            VehiclesVM.Cleanup();

            base.Cleanup();
        }

        #region Commands

        private RelayCommand closeCommand;

        public RelayCommand CloseCommand
        {
            get
            {
                if (closeCommand == null) closeCommand = new RelayCommand(() => OnClose(), () => CanClose());

                return closeCommand;
            }
        }

        private bool CanClose()
        {
            return true;
        }

        private void OnClose()
        {
            Logger.WriteDebug("AlarmWindow: CloseCommand");

            RaiseCloseRequestEvent();
        }

        #endregion //Commands

        #region Events

        public event EventHandler CloseRequested;

        private void RaiseCloseRequestEvent()
        {
            var handler = CloseRequested;
            if (handler != null) handler(this, new EventArgs());
        }

        #endregion //Events

        #region Validation

        #endregion //Validation

        #region Public Properties

        public bool IsAdminMode => ServiceLocator.Current.GetInstance<MainViewModel>().IsAdminMode;

        public Color ForegroundColor => Settings.Default.ForegroundColor;

        #region ViewModels

        public AlarmAlarmappViewModel AlarmappVM { get; }

        public AlarmDataViewModel DataVM { get; }

        public AlarmRouteViewModel RouteVM { get; }

        public AlarmTimerViewModel TimerVM { get; }

        public AlarmVehiclesViewModel VehiclesVM { get; }

        #endregion //ViewModels

        #endregion //Public Properties

        #region Public Functions

        #endregion

        #region Private Properties

        #endregion //Private Properties

        #region Private Funtions

        #endregion //Private Funtions        
    }
}