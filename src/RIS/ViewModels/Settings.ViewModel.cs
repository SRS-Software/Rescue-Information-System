#region

using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using RIS.Business;
using RIS.Core.Helper;
using RIS.Properties;
using RIS.Views;
using SRS.Utilities;
using Application = System.Windows.Application;
using MessageBox = RIS.Core.Helper.MessageBox;
using WindowState = Xceed.Wpf.Toolkit.WindowState;

#endregion

namespace RIS.ViewModels
{
    public sealed class SettingsViewModel : ViewModelBase
    {
        private readonly IBusiness _business;

        public SettingsViewModel()
        {
            try
            {
                _business = ServiceLocator.Current.GetInstance<IBusiness>();

                SettingsAlarmfaxVM = new SettingsAlarmfaxViewModel();
                SettingsAnzeigeVM = new SettingsAnzeigeViewModel();
                SettingsDatenschnittstelleVM = new SettingsDatenschnittstelleViewModel();

                //Messenger to open & close dialog
                Messenger.Default.Register<SettingsAaoDialog>(this, notification =>
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        AaoDialog_Content = notification;
                        AaoDialog_State = notification == null ? WindowState.Closed : WindowState.Open;
                    }));
                Messenger.Default.Register<SettingsAlarmappDialog>(this, notification =>
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        AlarmappDialog_Content = notification;
                        AlarmappDialog_State = notification == null ? WindowState.Closed : WindowState.Open;
                    }));
                Messenger.Default.Register<SettingsAmsDialog>(this, notification =>
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        AmsDialog_Content = notification;
                        AmsDialog_State = notification == null ? WindowState.Closed : WindowState.Open;
                    }));
                Messenger.Default.Register<SettingsFileprintDialog>(this, notification =>
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        FileprintDialog_Content = notification;
                        FileprintDialog_State = notification == null ? WindowState.Closed : WindowState.Open;
                    }));
                Messenger.Default.Register<SettingsFilterDialog>(this, notification =>
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        FilterDialog_Content = notification;
                        FilterDialog_State = notification == null ? WindowState.Closed : WindowState.Open;
                    }));
                Messenger.Default.Register<SettingsPrinterDialog>(this, notification =>
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        PrinterDialog_Content = notification;
                        PrinterDialog_State = notification == null ? WindowState.Closed : WindowState.Open;
                    }));
                Messenger.Default.Register<SettingsUserDialog>(this, notification =>
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        UserDialog_Content = notification;
                        UserDialog_State = notification == null ? WindowState.Closed : WindowState.Open;
                    }));
                Messenger.Default.Register<SettingsVehicleDialog>(this, notification =>
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        VehicleDialog_Content = notification;
                        VehicleDialog_State = notification == null ? WindowState.Closed : WindowState.Open;
                    }));
                Messenger.Default.Register<SettingsPagerDialog>(this, notification =>
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        PagerDialog_Content = notification;
                        PagerDialog_State = notification == null ? WindowState.Closed : WindowState.Open;
                    }));
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
            Logger.WriteDebug("SettingsWindow: CloseCommand");

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
                SettingsAlarmfaxVM.Save();
                SettingsAnzeigeVM.Save();
                SettingsDatenschnittstelleVM.Save();

                Settings.Default.Save();

                RaiseCloseRequestEvent();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand settingsExportCommand;

        public RelayCommand SettingsExportCommand
        {
            get
            {
                if (settingsExportCommand == null)
                    settingsExportCommand = new RelayCommand(() => OnSettingsExport(), () => CanSettingsExport());

                return settingsExportCommand;
            }
        }

        private bool CanSettingsExport()
        {
            return true;
        }

        private void OnSettingsExport()
        {
            try
            {
                Logger.WriteDebug("Settings: SettingsExportCommand");

                var _saveFileDialog = new SaveFileDialog();
                _saveFileDialog.Filter = "Setting files (*.setting)|*.setting";
                _saveFileDialog.AddExtension = true;
                _saveFileDialog.CheckPathExists = true;
                _saveFileDialog.InitialDirectory = Environment.SpecialFolder.MyComputer.ToString();
                if (_saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var _settings = new SettingsExport();

                    //Database Data
                    var _databasePath = @"C:\ProgramData\RIS\RISv7-DB.sdf";
                    _settings.DatabaseData = File.ReadAllBytes(_databasePath);

                    //User settings
                    var _userConfigFile = ConfigurationManager
                        .OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
                    using (var _streamReader = new StreamReader(_userConfigFile, Encoding.GetEncoding(1252)))
                    {
                        _settings.UserSettings = _streamReader.ReadToEnd();
                    }

                    Serializer.SerializeToFile(_settings, _saveFileDialog.FileName);
                    MessageBox.Show("Einstellungsdatei wurden erfolgreich erstellt.", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show(
                    "Leider ist ein unerwarteter Fehler aufgetreten. Bitte setzen Sie sich mit dem Support in Verbindung.\r\n\r\n" +
                    ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private RelayCommand settingsImportCommand;

        public RelayCommand SettingsImportCommand
        {
            get
            {
                if (settingsImportCommand == null)
                    settingsImportCommand = new RelayCommand(() => OnSettingsImport(), () => CanSettingsImport());

                return settingsImportCommand;
            }
        }

        private bool CanSettingsImport()
        {
            return true;
        }

        private void OnSettingsImport()
        {
            try
            {
                Logger.WriteDebug("Settings: SettingsImportCommand");

                var _openFileDialog = new OpenFileDialog();
                _openFileDialog.Filter = "Setting files (*.setting)|*.setting";
                _openFileDialog.CheckFileExists = true;
                _openFileDialog.InitialDirectory = Environment.SpecialFolder.MyComputer.ToString();
                if (_openFileDialog.ShowDialog() != DialogResult.OK) return;

                var _settings = Serializer.DeserializeFromFile<SettingsExport>(_openFileDialog.FileName);
                if (_settings.DatabaseData == null || _settings.DatabaseData.Length <= 0 ||
                    string.IsNullOrEmpty(_settings.UserSettings))
                {
                    MessageBox.Show("Einstellungsdatei konnte nicht geladen werden", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                var _result =
                    MessageBox.Show(
                        "Einstellungsdatei wurden erfolgreich gelesen.\r\nWollen Sie Ihre aktuellen Einstellungen wirklich überschreiben?",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (_result != MessageBoxResult.Yes) return;

                //Database Data
                var _databasePath = @"C:\ProgramData\RIS\RISv7-DB.sdf";
                File.Delete(_databasePath);
                File.WriteAllBytes(_databasePath, _settings.DatabaseData);

                //User settings
                var _userConfigFile = ConfigurationManager
                    .OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
                using (var _streamWriter = new StreamWriter(_userConfigFile))
                {
                    _streamWriter.Write(_settings.UserSettings);
                }

                Settings.Default.Reload();
                Settings.Default.Save();

                MessageBox.Show("Einstellungen wurden erfolgreich geladen.", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                RaiseCloseRequestEvent();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show(
                    "Leider ist ein unerwarteter Fehler aufgetreten. Bitte setzen Sie sich mit dem Support in Verbindung.\r\n\r\n" +
                    ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
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

        public string WindowTitel => "Einstellungen";

        public string Version => "Rescue-Information-System " + Assembly.GetExecutingAssembly().GetName().Version;

        #region ViewModels

        public SettingsAlarmfaxViewModel SettingsAlarmfaxVM { get; }

        public SettingsAnzeigeViewModel SettingsAnzeigeVM { get; }

        public SettingsDatenschnittstelleViewModel SettingsDatenschnittstelleVM { get; }

        #endregion //ViewModels

        #region Dialogs

        private WindowState aaoDialog_State;

        public WindowState AaoDialog_State
        {
            get => aaoDialog_State;
            set
            {
                if (aaoDialog_State == value) return;

                aaoDialog_State = value;

                RaisePropertyChanged(() => AaoDialog_State);
            }
        }

        private object aaoDialog_Content;

        public object AaoDialog_Content
        {
            get => aaoDialog_Content;
            set
            {
                if (aaoDialog_Content == value) return;

                aaoDialog_Content = value;

                RaisePropertyChanged(() => AaoDialog_Content);
            }
        }

        private WindowState alarmappDialog_State;

        public WindowState AlarmappDialog_State
        {
            get => alarmappDialog_State;
            set
            {
                if (alarmappDialog_State == value) return;

                alarmappDialog_State = value;

                RaisePropertyChanged(() => AlarmappDialog_State);
            }
        }

        private object alarmappDialog_Content;

        public object AlarmappDialog_Content
        {
            get => alarmappDialog_Content;
            set
            {
                if (alarmappDialog_Content == value) return;

                alarmappDialog_Content = value;

                RaisePropertyChanged(() => AlarmappDialog_Content);
            }
        }

        private WindowState amsDialog_State;

        public WindowState AmsDialog_State
        {
            get => amsDialog_State;
            set
            {
                if (amsDialog_State == value) return;

                amsDialog_State = value;

                RaisePropertyChanged(() => AmsDialog_State);
            }
        }

        private object amsDialog_Content;

        public object AmsDialog_Content
        {
            get => amsDialog_Content;
            set
            {
                if (amsDialog_Content == value) return;

                amsDialog_Content = value;

                RaisePropertyChanged(() => AmsDialog_Content);
            }
        }

        private WindowState fileprintDialog_State;

        public WindowState FileprintDialog_State
        {
            get => fileprintDialog_State;
            set
            {
                if (fileprintDialog_State == value) return;

                fileprintDialog_State = value;

                RaisePropertyChanged(() => FileprintDialog_State);
            }
        }

        private object fileprintDialog_Content;

        public object FileprintDialog_Content
        {
            get => fileprintDialog_Content;
            set
            {
                if (fileprintDialog_Content == value) return;

                fileprintDialog_Content = value;

                RaisePropertyChanged(() => FileprintDialog_Content);
            }
        }

        private WindowState filterDialog_State;

        public WindowState FilterDialog_State
        {
            get => filterDialog_State;
            set
            {
                if (filterDialog_State == value) return;

                filterDialog_State = value;

                RaisePropertyChanged(() => FilterDialog_State);
            }
        }

        private object filterDialog_Content;

        public object FilterDialog_Content
        {
            get => filterDialog_Content;
            set
            {
                if (filterDialog_Content == value) return;

                filterDialog_Content = value;

                RaisePropertyChanged(() => FilterDialog_Content);
            }
        }

        private WindowState printerDialog_State;

        public WindowState PrinterDialog_State
        {
            get => printerDialog_State;
            set
            {
                if (printerDialog_State == value) return;

                printerDialog_State = value;

                RaisePropertyChanged(() => PrinterDialog_State);
            }
        }

        private object printerDialog_Content;

        public object PrinterDialog_Content
        {
            get => printerDialog_Content;
            set
            {
                if (printerDialog_Content == value) return;

                printerDialog_Content = value;

                RaisePropertyChanged(() => PrinterDialog_Content);
            }
        }

        private WindowState userDialog_State;

        public WindowState UserDialog_State
        {
            get => userDialog_State;
            set
            {
                if (userDialog_State == value) return;

                userDialog_State = value;

                RaisePropertyChanged(() => UserDialog_State);
            }
        }

        private object userDialog_Content;

        public object UserDialog_Content
        {
            get => userDialog_Content;
            set
            {
                if (userDialog_Content == value) return;

                userDialog_Content = value;

                RaisePropertyChanged(() => UserDialog_Content);
            }
        }

        private WindowState vehicleDialog_State;

        public WindowState VehicleDialog_State
        {
            get => vehicleDialog_State;
            set
            {
                if (vehicleDialog_State == value) return;

                vehicleDialog_State = value;

                RaisePropertyChanged(() => VehicleDialog_State);
            }
        }

        private object vehicleDialog_Content;

        public object VehicleDialog_Content
        {
            get => vehicleDialog_Content;
            set
            {
                if (vehicleDialog_Content == value) return;

                vehicleDialog_Content = value;

                RaisePropertyChanged(() => VehicleDialog_Content);
            }
        }

        private WindowState pagerDialog_State;

        public WindowState PagerDialog_State
        {
            get => pagerDialog_State;
            set
            {
                if (pagerDialog_State == value) return;

                pagerDialog_State = value;

                RaisePropertyChanged(() => PagerDialog_State);
            }
        }

        private object pagerDialog_Content;

        public object PagerDialog_Content
        {
            get => pagerDialog_Content;
            set
            {
                if (pagerDialog_Content == value) return;

                pagerDialog_Content = value;

                RaisePropertyChanged(() => PagerDialog_Content);
            }
        }

        #endregion //Dialogs

        #endregion //Public Properties

        #region Public Functions

        #endregion

        #region Private Properties

        #endregion //Private Properties

        #region Private Funtions

        #endregion //Private Funtions
    }
}