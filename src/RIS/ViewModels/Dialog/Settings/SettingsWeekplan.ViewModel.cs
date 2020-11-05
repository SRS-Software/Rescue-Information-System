#region

using System;
using System.Reflection;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using SRS.Utilities;
using MessageBox = RIS.Core.Helper.MessageBox;

#endregion

namespace RIS.ViewModels
{
    public class SettingsWeekplanViewModel : ViewModelBase
    {
        public SettingsWeekplanViewModel()
        {
            try
            {
                Result = false;
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
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
            Logger.WriteDebug("WeekplanWindow: CloseCommand");

            RaiseCloseRequestEvent();
        }

        private RelayCommand saveCommand;

        public RelayCommand SaveCommand
        {
            get
            {
                if (saveCommand == null) saveCommand = new RelayCommand(() => OnSave(), () => CanSave());

                return saveCommand;
            }
        }

        private bool CanSave()
        {
            return true;
        }

        private void OnSave()
        {
            try
            {
                Logger.WriteDebug("WeekplanWindow: SaveCommand");

                Result = true;
                RaiseCloseRequestEvent();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
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

        public bool Result { get; private set; }

        public string WindowTitel => "Zeitplan";

        public DateTime start = new DateTime(2015, 01, 01, 08, 0, 0);

        public DateTime Start
        {
            get => start;
            set
            {
                if (start == value) return;

                start = value;
                RaisePropertyChanged(() => Start);
            }
        }

        public DateTime stop = new DateTime(2015, 01, 01, 17, 0, 0);

        public DateTime Stop
        {
            get => stop;
            set
            {
                if (stop == value) return;

                stop = value;
                RaisePropertyChanged(() => Stop);
            }
        }

        #endregion //Public Properties

        #region Public Functions

        #endregion //Public Functions

        #region Private Properties

        #endregion //Private Properties

        #region Private Functions

        #endregion //Private Funtions
    }
}