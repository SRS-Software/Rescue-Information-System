#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
using RIS.Business;
using RIS.Core;
using RIS.Core.Fax;
using RIS.Factories;
using RIS.Model;
using RIS.Properties;
using RIS.Views;
using SRS.Utilities;
using MessageBox = RIS.Core.Helper.MessageBox;

#endregion

namespace RIS.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IBusiness _business;
        private readonly IDecoderService _decoderService;
        private readonly IFaxService _faxService;
        private readonly IMailService _mailService;
        private readonly IMonitorService _monitorService;
        private readonly IWatchdogService _watchdogService;

        public MainViewModel()
        {
            try
            {
                _business = ServiceLocator.Current.GetInstance<IBusiness>();
                _mailService = ServiceLocator.Current.GetInstance<IMailService>();
                _monitorService = ServiceLocator.Current.GetInstance<IMonitorService>();
                _watchdogService = ServiceLocator.Current.GetInstance<IWatchdogService>();

                _faxService = ServiceLocator.Current.GetInstance<IFaxService>();
                if (_faxService != null)
                    _faxService.EinsatzCreated += (sender, e) => DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        faxService_EinsatzCreated(sender, e);
                    });

                _decoderService = ServiceLocator.Current.GetInstance<IDecoderService>();
                if (_decoderService != null)
                    _decoderService.StatusChanged += (sender, e) => DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        switch (e.Number)
                        {
                            case 1:
                                DecoderStatus1 = Settings.Default.Decoder1_Mode +
                                                 (Settings.Default.Decoder1_Mode == DecoderMode.OFF
                                                     ? ""
                                                     : $"[{e.Status}]");
                                break;
                            case 2:
                                DecoderStatus2 = Settings.Default.Decoder2_Mode +
                                                 (Settings.Default.Decoder2_Mode == DecoderMode.OFF
                                                     ? ""
                                                     : $"[{e.Status}]");
                                break;
                        }
                    });

#if DEBUG
                IsAdminMode = true;
#endif
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #region Public Functions

        public void Initialize()
        {
            RaisePropertyChanged(() => ForegroundColor);
        }

        #endregion //Public Functions

        #region Private Functions

        private void faxService_EinsatzCreated(object sender, EinsatzCreatedEventArgs e)
        {
            if (e == null || e.Einsatz == null) return;

            var _tempTime = e.Einsatz.AlarmTime;
            //Set new AlarmTime if AMS was activ in the last 5min
            var _pagerMessages = ServiceLocator.Current.GetInstance<MainPagersViewModel>()
                .PagerMessages.OrderByDescending(a => a.Time).ToList();
            if (_pagerMessages != null)
            {
                foreach (var _message in _pagerMessages)
                {
                    //Stop if time is longer ago then 5 minutes 
                    if ((DateTime.Now - _message.Time).Minutes > 5) break;

                    //Change time only if prio
                    if (_message.Priority) e.Einsatz.AlarmTime = _message.Time;
                }

                //Logging
                if (e.Einsatz.AlarmTime != _tempTime)
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                        "Einsatz -> AlarmTime changed to " + e.Einsatz.AlarmTime);
            }

            //Show Window
            var _alarmWindow = new AlarmWindow(e.Einsatz);
            _alarmWindow.Topmost = IsAdminMode ? false : true;
            _alarmWindow.Show();
        }

        #endregion //Private Funtions

        #region Commands

        private RelayCommand exitCommand;

        public RelayCommand ExitCommand
        {
            get
            {
                if (exitCommand == null) exitCommand = new RelayCommand(() => OnExit(), () => CanExit());

                return exitCommand;
            }
        }

        private bool CanExit()
        {
            return IsAdminMode;
        }

        private void OnExit()
        {
            Logger.WriteDebug("RIS: ExitCommand");

            ServiceFactory.Stop();

            IsExit = true;
            Application.Current.Shutdown();
        }

        private RelayCommand showSettingsCommand;

        public RelayCommand ShowSettingsCommand
        {
            get
            {
                if (showSettingsCommand == null)
                    showSettingsCommand = new RelayCommand(() => OnShowSettings(), () => CanShowSettings());

                return showSettingsCommand;
            }
        }

        private bool CanShowSettings()
        {
            return IsAdminMode;
        }

        private void OnShowSettings()
        {
            try
            {
                Logger.WriteDebug("RIS: ShowSettingsCommand");

                //Stop Services and Views
                ServiceFactory.Stop();

                //Show settingWindow
                var _settingsWindow = new SettingsWindow();
                _settingsWindow.ShowDialog();

                //Start Services and Views
                ServiceFactory.Start();
                ViewModelFactory.Initialize();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        
        private RelayCommand adminCommand;

        public RelayCommand AdminCommand
        {
            get
            {
                if (adminCommand == null) adminCommand = new RelayCommand(() => OnAdmin(), () => CanAdmin());

                return adminCommand;
            }
        }

        private bool CanAdmin()
        {
            return true;
        }

        private void OnAdmin()
        {
            try
            {
                Logger.WriteDebug("RIS: AdminCommand");

                if (IsAdminMode)
                {
                    IsAdminMode = false;
                }
                else
                {
                    var _adminWindow = new MainAdminWindow();
                    if (_adminWindow.ShowDialog() == true) IsAdminMode = true;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand showAlarmCommand;

        public RelayCommand ShowAlarmCommand
        {
            get
            {
                if (showAlarmCommand == null)
                    showAlarmCommand = new RelayCommand(() => OnShowAlarm(), () => CanShowAlarm());

                return showAlarmCommand;
            }
        }

        private bool CanShowAlarm()
        {
            return IsAdminMode;
        }

        private void OnShowAlarm()
        {
            try
            {
                Logger.WriteDebug("RIS: ShowAlarmCommand");

                var _testEinsatz = new Einsatz
                {
                    AbsenderValid = true,
                    AlarmTime = DateTime.Now,
                    Ort = "83278 Traunstein",
                    Straße = "Empfing",
                    Hausnummer = "15",
                    Objekt = "Tierheim Traunstein",
                    Schlagwort = "Brand Wohnhaus",
                    Stichwort = "B4",
                    Bemerkung = "Flammen schlagen aus dem Dach.Keine Personen mehr im Gebäude!Dies ist ein Test!!"
                };
                _testEinsatz.Einsatzmittel = new List<Vehicle>(_business.GetVehiclesAreEinsatzmittel().Take(3));

                var _alarmWindow = new AlarmWindow(_testEinsatz);
                _alarmWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand dataVehiclesResetCommand;

        public RelayCommand DataVehiclesResetCommand
        {
            get
            {
                if (dataVehiclesResetCommand == null)
                    dataVehiclesResetCommand =
                        new RelayCommand(() => OnDataVehiclesReset(), () => CanDataVehiclesReset());

                return dataVehiclesResetCommand;
            }
        }

        private bool CanDataVehiclesReset()
        {
            return IsAdminMode;
        }

        private void OnDataVehiclesReset()
        {
            try
            {
                Logger.WriteDebug("RIS: DataVehiclesResetCommand");

                ServiceLocator.Current.GetInstance<MainVehiclesViewModel>().Reset();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand dataPagersResetCommand;

        public RelayCommand DataPagersResetCommand
        {
            get
            {
                if (dataPagersResetCommand == null)
                    dataPagersResetCommand = new RelayCommand(() => OnDataPagersReset(), () => CanDataPagersReset());

                return dataPagersResetCommand;
            }
        }

        private bool CanDataPagersReset()
        {
            return IsAdminMode;
        }

        private void OnDataPagersReset()
        {
            try
            {
                Logger.WriteDebug("RIS: DataPagersResetCommand");

                ServiceLocator.Current.GetInstance<MainPagersViewModel>().Reset();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand decoderConnectCommand;

        public RelayCommand DecoderConnectCommand
        {
            get
            {
                if (decoderConnectCommand == null)
                    decoderConnectCommand = new RelayCommand(() => OnDecoderConnect(), () => CanDecoderConnect());

                return decoderConnectCommand;
            }
        }

        private bool CanDecoderConnect()
        {
            return IsAdminMode;
        }

        private void OnDecoderConnect()
        {
            try
            {
                Logger.WriteDebug("RIS: DecoderConnectCommand");

                _decoderService.Stop();
                _decoderService.Start();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand showLogCommand;

        public RelayCommand ShowLogCommand
        {
            get
            {
                if (showLogCommand == null) showLogCommand = new RelayCommand(() => OnShowLog(), () => CanShowLog());

                return showLogCommand;
            }
        }

        private bool CanShowLog()
        {
            return IsAdminMode;
        }

        private void OnShowLog()
        {
            try
            {
                Logger.WriteDebug("RIS: ShowLogCommand");

                string _logText;
                using (var _streamReader = new StreamReader(Logger.LogFile, Encoding.GetEncoding(1252)))
                {
                    _logText = _streamReader.ReadToEnd();
                }

                var _textWindow = new TextWindow(_logText);
                _textWindow.Show();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private RelayCommand logoutCommand;

        public RelayCommand LogoutCommand
        {
            get
            {
                if (logoutCommand == null) logoutCommand = new RelayCommand(() => OnLogout(), () => CanLogout());

                return logoutCommand;
            }
        }

        private bool CanLogout()
        {
            return IsAdminMode;
        }

        private void OnLogout()
        {
            try
            {
                Logger.WriteDebug("RIS: LogoutCommand");
                //_webService.Logout();

                OnExit();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand helpCommand;

        public RelayCommand HelpCommand
        {
            get
            {
                if (helpCommand == null) helpCommand = new RelayCommand(() => OnHelp(), () => CanHelp());

                return helpCommand;
            }
        }

        private bool CanHelp()
        {
            return IsAdminMode;
        }

        private void OnHelp()
        {
            try
            {
                Logger.WriteDebug("RIS: HelpCommand");

                Process.Start(new ProcessStartInfo(@"https://github.com/SRS-Software/Rescue-Information-System/wiki"));
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

        #endregion //Events

        #region Validation

        #endregion //Validation

        #region Public Properties

        public bool IsExit { get; set; }

        private bool isAdminMode;

        public bool IsAdminMode
        {
            get => isAdminMode;
            set
            {
                if (isAdminMode == value) return;

                isAdminMode = value;

                RaisePropertyChanged(() => IsAdminMode);
                RaisePropertyChanged(() => MenuAdminStateHeader);
            }
        }

        public string MenuAdminStateHeader => IsAdminMode ? "Admin - Logout" : " Admin - Login";

        public Color ForegroundColor => Settings.Default.ForegroundColor;

        private string decoderStatus1;

        public string DecoderStatus1
        {
            get => decoderStatus1;
            set
            {
                if (decoderStatus1 == value) return;

                decoderStatus1 = value;

                RaisePropertyChanged(() => DecoderStatus1);
            }
        }

        private string decoderStatus2;

        public string DecoderStatus2
        {
            get => decoderStatus2;
            set
            {
                if (decoderStatus2 == value) return;

                decoderStatus2 = value;

                RaisePropertyChanged(() => DecoderStatus2);
            }
        }

        private string updateStatus;

        public string UpdateStatus
        {
            get => updateStatus;
            set
            {
                if (updateStatus == value) return;

                updateStatus = value;
                RaisePropertyChanged(() => UpdateStatus);
                RaisePropertyChanged(() => UpdateVisibility);
            }
        }

        public Visibility UpdateVisibility
        {
            get
            {
                if (string.IsNullOrEmpty(UpdateStatus)) return Visibility.Collapsed;

                return Visibility.Visible;
            }
        }

        #endregion //Public Properties

        #region Private Properties

        #endregion //Private Properties
    }
}