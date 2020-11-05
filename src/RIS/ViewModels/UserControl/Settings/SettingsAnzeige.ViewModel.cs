#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
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
using RIS.Core;
using RIS.Core.Helper;
using RIS.Model;
using RIS.Properties;
using RIS.Views;
using SRS.Utilities;
using Application = System.Windows.Forms.Application;
using MessageBox = RIS.Core.Helper.MessageBox;

#endregion

namespace RIS.ViewModels
{
    public class SettingsAnzeigeViewModel : ViewModelBase
    {
        private readonly IBusiness business;
        private readonly IMapService mapService;

        public SettingsAnzeigeViewModel()
        {
            try
            {
                business = ServiceLocator.Current.GetInstance<IBusiness>();
                mapService = ServiceLocator.Current.GetInstance<IMapService>();

                //Monitor Settings     
                monitorSettings = Serializer.Deserialize<SettingsMonitor>(Settings.Default.Monitor_Weekplan);

                //FMS Settings         
                fmsSettings = Serializer.Deserialize<SettingsFms>(Settings.Default.FmsSettings);

                LoadVehicleList();
                LoadPagerList();

                //Messenger refresh affected list             
                Messenger.Default.Register<SettingsVehicleDialog>(this,
                    notification => DispatcherHelper.CheckBeginInvokeOnUI(() => { LoadVehicleList(); }));
                Messenger.Default.Register<SettingsPagerDialog>(this,
                    notification => DispatcherHelper.CheckBeginInvokeOnUI(() => { LoadPagerList(); }));
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #region Public Functions

        public void Save()
        {
            Settings.Default.Monitor_Weekplan = Serializer.Serialize(MonitorSettings);
            Settings.Default.FmsSettings = Serializer.Serialize(FmsSettings);
        }

        #endregion //Public Functions

        #region Commands

        #region Allgemein

        private RelayCommand ticker_FileBrowseCommand;

        [Display(Description = "")]
        public RelayCommand Ticker_FileBrowseCommand
        {
            get
            {
                if (ticker_FileBrowseCommand == null)
                    ticker_FileBrowseCommand =
                        new RelayCommand(() => OnTicker_FileBrowse(), () => CanTicker_FileBrowse());

                return ticker_FileBrowseCommand;
            }
        }

        private bool CanTicker_FileBrowse()
        {
            return true;
        }

        private void OnTicker_FileBrowse()
        {
            Logger.WriteDebug("Settings: Ticker_FileBrowseCommand");

            try
            {
                var _openFileDialog = new OpenFileDialog
                {
                    Title = "Wählen Sie die gewünschte Textdatei aus:",
                    RestoreDirectory = true,
                    InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath),
                    Filter = "Textdatei (*.txt)|*.txt",
                    CheckFileExists = true
                };

                if (_openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Settings.Default.Ticker_IsFile = true;
                    Settings.Default.Ticker_Text = _openFileDialog.FileName;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion //Allgemein

        #region Monitor

        private RelayCommand addTimeMondayCommand;

        [Display(Description = "Öffnet den Dialog zum Hinzufügen einer neuen Einschaltzeit")]
        public RelayCommand AddTimeMondayCommand
        {
            get
            {
                if (addTimeMondayCommand == null)
                    addTimeMondayCommand = new RelayCommand(() => OnAddTimeMonday(), () => CanAddTimeMonday());

                return addTimeMondayCommand;
            }
        }

        private bool CanAddTimeMonday()
        {
            if (MonitorSettings == null) return false;

            if (MonitorSettings.Monday == null) return false;

            return true;
        }

        private void OnAddTimeMonday()
        {
            try
            {
                Logger.WriteDebug("Settings: AddTimeMondayCommand");

                var _weekplanDialog = new SettingsWeekplanDialog();
                if (_weekplanDialog.ShowDialog() == true)
                {
                    var _viewModel = _weekplanDialog.DataContext as SettingsWeekplanViewModel;
                    if (_viewModel == null) return;

                    var _monitorItem =
                        new SettingsMonitor.MonitorItem(_viewModel.Start, _viewModel.Stop);
                    MonitorSettings.Monday.Add(_monitorItem);

                    SaveMonitorSettings();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand<object> deleteTimeMondayCommand;

        [Display(Description = "Entfernt diese Einschaltzeit")]
        public RelayCommand<object> DeleteTimeMondayCommand
        {
            get
            {
                if (deleteTimeMondayCommand == null)
                    deleteTimeMondayCommand = new RelayCommand<object>(param => OnDeleteTimeMonday(param),
                        param => CanDeleteTimeMonday(param));

                return deleteTimeMondayCommand;
            }
        }

        private bool CanDeleteTimeMonday(object param)
        {
            if (param == null) return false;

            if (MonitorSettings == null) return false;

            if (MonitorSettings.Monday == null) return false;

            return true;
        }

        private void OnDeleteTimeMonday(object param)
        {
            try
            {
                Logger.WriteDebug("Settings: DeleteTimeMondayCommand");

                MonitorSettings.Monday.Remove((SettingsMonitor.MonitorItem) param);

                SaveMonitorSettings();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand addTimeTuesdayCommand;

        [Display(Description = "Öffnet den Dialog zum Hinzufügen einer neuen Einschaltzeit")]
        public RelayCommand AddTimeTuesdayCommand
        {
            get
            {
                if (addTimeTuesdayCommand == null)
                    addTimeTuesdayCommand = new RelayCommand(() => OnAddTimeTuesday(), () => CanAddTimeTuesday());

                return addTimeTuesdayCommand;
            }
        }

        private bool CanAddTimeTuesday()
        {
            if (MonitorSettings == null) return false;

            if (MonitorSettings.Tuesday == null) return false;

            return true;
        }

        private void OnAddTimeTuesday()
        {
            try
            {
                Logger.WriteDebug("Settings: AddTimeTuesdayCommand");

                var _weekplanDialog = new SettingsWeekplanDialog();
                if (_weekplanDialog.ShowDialog() == true)
                {
                    var _viewModel = _weekplanDialog.DataContext as SettingsWeekplanViewModel;
                    if (_viewModel == null) return;

                    var _monitorItem =
                        new SettingsMonitor.MonitorItem(_viewModel.Start, _viewModel.Stop);
                    MonitorSettings.Tuesday.Add(_monitorItem);

                    SaveMonitorSettings();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand<object> deleteTimeTuesdayCommand;

        [Display(Description = "Entfernt diese Einschaltzeit")]
        public RelayCommand<object> DeleteTimeTuesdayCommand
        {
            get
            {
                if (deleteTimeTuesdayCommand == null)
                    deleteTimeTuesdayCommand = new RelayCommand<object>(param => OnDeleteTimeTuesday(param),
                        param => CanDeleteTimeTuesday(param));

                return deleteTimeTuesdayCommand;
            }
        }

        private bool CanDeleteTimeTuesday(object param)
        {
            if (param == null) return false;

            if (MonitorSettings == null) return false;

            if (MonitorSettings.Tuesday == null) return false;

            return true;
        }

        private void OnDeleteTimeTuesday(object param)
        {
            try
            {
                Logger.WriteDebug("Settings: DeleteTimeTuesdayCommand");

                MonitorSettings.Tuesday.Remove((SettingsMonitor.MonitorItem) param);

                SaveMonitorSettings();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand addTimeWednesdayCommand;

        [Display(Description = "Öffnet den Dialog zum Hinzufügen einer neuen Einschaltzeit")]
        public RelayCommand AddTimeWednesdayCommand
        {
            get
            {
                if (addTimeWednesdayCommand == null)
                    addTimeWednesdayCommand = new RelayCommand(() => OnAddTimeWednesday(), () => CanAddTimeWednesday());

                return addTimeWednesdayCommand;
            }
        }

        private bool CanAddTimeWednesday()
        {
            if (MonitorSettings == null) return false;

            if (MonitorSettings.Wednesday == null) return false;

            return true;
        }

        private void OnAddTimeWednesday()
        {
            try
            {
                Logger.WriteDebug("Settings: AddTimeWednesdayCommand");

                var _weekplanDialog = new SettingsWeekplanDialog();
                if (_weekplanDialog.ShowDialog() == true)
                {
                    var _viewModel = _weekplanDialog.DataContext as SettingsWeekplanViewModel;
                    if (_viewModel == null) return;

                    var _monitorItem =
                        new SettingsMonitor.MonitorItem(_viewModel.Start, _viewModel.Stop);
                    MonitorSettings.Wednesday.Add(_monitorItem);

                    SaveMonitorSettings();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand<object> deleteTimeWednesdayCommand;

        [Display(Description = "Entfernt diese Einschaltzeit")]
        public RelayCommand<object> DeleteTimeWednesdayCommand
        {
            get
            {
                if (deleteTimeWednesdayCommand == null)
                    deleteTimeWednesdayCommand = new RelayCommand<object>(param => OnDeleteTimeWednesday(param),
                        param => CanDeleteTimeWednesday(param));

                return deleteTimeWednesdayCommand;
            }
        }

        private bool CanDeleteTimeWednesday(object param)
        {
            if (param == null) return false;

            if (MonitorSettings == null) return false;

            if (MonitorSettings.Wednesday == null) return false;

            return true;
        }

        private void OnDeleteTimeWednesday(object param)
        {
            try
            {
                Logger.WriteDebug("Settings: DeleteTimeWednesdayCommand");

                MonitorSettings.Wednesday.Remove((SettingsMonitor.MonitorItem) param);

                SaveMonitorSettings();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand addTimeThursdayCommand;

        [Display(Description = "Öffnet den Dialog zum Hinzufügen einer neuen Einschaltzeit")]
        public RelayCommand AddTimeThursdayCommand
        {
            get
            {
                if (addTimeThursdayCommand == null)
                    addTimeThursdayCommand = new RelayCommand(() => OnAddTimeThursday(), () => CanAddTimeThursday());

                return addTimeThursdayCommand;
            }
        }

        private bool CanAddTimeThursday()
        {
            if (MonitorSettings == null) return false;

            if (MonitorSettings.Thursday == null) return false;

            return true;
        }

        private void OnAddTimeThursday()
        {
            try
            {
                Logger.WriteDebug("Settings: AddTimeThursdayCommand");

                var _weekplanDialog = new SettingsWeekplanDialog();
                if (_weekplanDialog.ShowDialog() == true)
                {
                    var _viewModel = _weekplanDialog.DataContext as SettingsWeekplanViewModel;
                    if (_viewModel == null) return;

                    var _monitorItem =
                        new SettingsMonitor.MonitorItem(_viewModel.Start, _viewModel.Stop);
                    MonitorSettings.Thursday.Add(_monitorItem);

                    SaveMonitorSettings();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand<object> deleteTimeThursdayCommand;

        [Display(Description = "Entfernt diese Einschaltzeit")]
        public RelayCommand<object> DeleteTimeThursdayCommand
        {
            get
            {
                if (deleteTimeThursdayCommand == null)
                    deleteTimeThursdayCommand = new RelayCommand<object>(param => OnDeleteTimeThursday(param),
                        param => CanDeleteTimeThursday(param));

                return deleteTimeThursdayCommand;
            }
        }

        private bool CanDeleteTimeThursday(object param)
        {
            if (param == null) return false;

            if (MonitorSettings == null) return false;

            if (MonitorSettings.Thursday == null) return false;

            return true;
        }

        private void OnDeleteTimeThursday(object param)
        {
            try
            {
                Logger.WriteDebug("Settings: DeleteTimeThursdayCommand");

                MonitorSettings.Thursday.Remove((SettingsMonitor.MonitorItem) param);

                SaveMonitorSettings();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand addTimeFridayCommand;

        [Display(Description = "Öffnet den Dialog zum Hinzufügen einer neuen Einschaltzeit")]
        public RelayCommand AddTimeFridayCommand
        {
            get
            {
                if (addTimeFridayCommand == null)
                    addTimeFridayCommand = new RelayCommand(() => OnAddTimeFriday(), () => CanAddTimeFriday());

                return addTimeFridayCommand;
            }
        }

        private bool CanAddTimeFriday()
        {
            if (MonitorSettings == null) return false;

            if (MonitorSettings.Friday == null) return false;

            return true;
        }

        private void OnAddTimeFriday()
        {
            try
            {
                Logger.WriteDebug("Settings: AddTimeFridayCommand");

                var _weekplanDialog = new SettingsWeekplanDialog();
                if (_weekplanDialog.ShowDialog() == true)
                {
                    var _viewModel = _weekplanDialog.DataContext as SettingsWeekplanViewModel;
                    if (_viewModel == null) return;

                    var _monitorItem =
                        new SettingsMonitor.MonitorItem(_viewModel.Start, _viewModel.Stop);
                    MonitorSettings.Friday.Add(_monitorItem);

                    SaveMonitorSettings();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand<object> deleteTimeFridayCommand;

        [Display(Description = "Entfernt diese Einschaltzeit")]
        public RelayCommand<object> DeleteTimeFridayCommand
        {
            get
            {
                if (deleteTimeFridayCommand == null)
                    deleteTimeFridayCommand = new RelayCommand<object>(param => OnDeleteTimeFriday(param),
                        param => CanDeleteTimeFriday(param));

                return deleteTimeFridayCommand;
            }
        }

        private bool CanDeleteTimeFriday(object param)
        {
            if (param == null) return false;

            if (MonitorSettings == null) return false;

            if (MonitorSettings.Friday == null) return false;

            return true;
        }

        private void OnDeleteTimeFriday(object param)
        {
            try
            {
                Logger.WriteDebug("Settings: DeleteTimeFridayCommand");

                MonitorSettings.Friday.Remove((SettingsMonitor.MonitorItem) param);

                SaveMonitorSettings();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand addTimeSaturdayCommand;

        [Display(Description = "Öffnet den Dialog zum Hinzufügen einer neuen Einschaltzeit")]
        public RelayCommand AddTimeSaturdayCommand
        {
            get
            {
                if (addTimeSaturdayCommand == null)
                    addTimeSaturdayCommand = new RelayCommand(() => OnAddTimeSaturday(), () => CanAddTimeSaturday());

                return addTimeSaturdayCommand;
            }
        }

        private bool CanAddTimeSaturday()
        {
            if (MonitorSettings == null) return false;

            if (MonitorSettings.Saturday == null) return false;

            return true;
        }

        private void OnAddTimeSaturday()
        {
            try
            {
                Logger.WriteDebug("Settings: AddTimeSaturdayCommand");

                var _weekplanDialog = new SettingsWeekplanDialog();
                if (_weekplanDialog.ShowDialog() == true)
                {
                    var _viewModel = _weekplanDialog.DataContext as SettingsWeekplanViewModel;
                    if (_viewModel == null) return;

                    var _monitorItem =
                        new SettingsMonitor.MonitorItem(_viewModel.Start, _viewModel.Stop);
                    MonitorSettings.Saturday.Add(_monitorItem);

                    SaveMonitorSettings();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand<object> deleteTimeSaturdayCommand;

        [Display(Description = "Entfernt diese Einschaltzeit")]
        public RelayCommand<object> DeleteTimeSaturdayCommand
        {
            get
            {
                if (deleteTimeSaturdayCommand == null)
                    deleteTimeSaturdayCommand = new RelayCommand<object>(param => OnDeleteTimeSaturday(param),
                        param => CanDeleteTimeSaturday(param));

                return deleteTimeSaturdayCommand;
            }
        }

        private bool CanDeleteTimeSaturday(object param)
        {
            if (param == null) return false;

            if (MonitorSettings == null) return false;

            if (MonitorSettings.Saturday == null) return false;

            return true;
        }

        private void OnDeleteTimeSaturday(object param)
        {
            try
            {
                Logger.WriteDebug("Settings: DeleteTimeSaturdayCommand");

                MonitorSettings.Saturday.Remove((SettingsMonitor.MonitorItem) param);

                SaveMonitorSettings();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand addTimeSundayCommand;

        [Display(Description = "Öffnet den Dialog zum Hinzufügen einer neuen Einschaltzeit")]
        public RelayCommand AddTimeSundayCommand
        {
            get
            {
                if (addTimeSundayCommand == null)
                    addTimeSundayCommand = new RelayCommand(() => OnAddTimeSunday(), () => CanAddTimeSunday());

                return addTimeSundayCommand;
            }
        }

        private bool CanAddTimeSunday()
        {
            if (MonitorSettings == null) return false;

            if (MonitorSettings.Sunday == null) return false;

            return true;
        }

        private void OnAddTimeSunday()
        {
            try
            {
                Logger.WriteDebug("Settings: AddTimeSundayCommand");

                var _weekplanDialog = new SettingsWeekplanDialog();
                if (_weekplanDialog.ShowDialog() == true)
                {
                    var _viewModel = _weekplanDialog.DataContext as SettingsWeekplanViewModel;
                    if (_viewModel == null) return;

                    var _monitorItem =
                        new SettingsMonitor.MonitorItem(_viewModel.Start, _viewModel.Stop);
                    MonitorSettings.Sunday.Add(_monitorItem);

                    SaveMonitorSettings();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand<object> deleteTimeSundayCommand;

        [Display(Description = "Entfernt diese Einschaltzeit")]
        public RelayCommand<object> DeleteTimeSundayCommand
        {
            get
            {
                if (deleteTimeSundayCommand == null)
                    deleteTimeSundayCommand = new RelayCommand<object>(param => OnDeleteTimeSunday(param),
                        param => CanDeleteTimeSunday(param));

                return deleteTimeSundayCommand;
            }
        }

        private bool CanDeleteTimeSunday(object param)
        {
            if (param == null) return false;

            if (MonitorSettings == null) return false;

            if (MonitorSettings.Sunday == null) return false;

            return true;
        }

        private void OnDeleteTimeSunday(object param)
        {
            try
            {
                Logger.WriteDebug("Settings: DeleteTimeSundayCommand");

                MonitorSettings.Sunday.Remove((SettingsMonitor.MonitorItem) param);

                SaveMonitorSettings();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion //Monitor

        #region Route

        private RelayCommand route_GetCoordinatesCommand;

        [Display(Description =
            "Ermittelt den Startpunkt der für die Routenplanung verwendet wird.\nWird kein Startpunkt festgelegt, erfolgt auch keine Routenplanung")]
        public RelayCommand Route_GetCoordinatesCommand
        {
            get
            {
                if (route_GetCoordinatesCommand == null)
                    route_GetCoordinatesCommand =
                        new RelayCommand(() => OnRoute_GetCoordinates(), () => CanRoute_GetCoordinates());

                return route_GetCoordinatesCommand;
            }
        }

        private bool CanRoute_GetCoordinates()
        {
            if (string.IsNullOrWhiteSpace(Route_Zip)) return false;

            if (string.IsNullOrWhiteSpace(Route_City)) return false;

            if (string.IsNullOrWhiteSpace(Route_Street)) return false;

            if (string.IsNullOrWhiteSpace(Route_Housenumber)) return false;

            return true;
        }

        private void OnRoute_GetCoordinates()
        {
            Logger.WriteDebug("Settings: Route_GetCoordinatesCommand");

            var _address = $"{Route_Zip} {Route_City},{Route_Street} {Route_Housenumber}";
            var _location = mapService.Geocode(_address);
            if (!string.IsNullOrWhiteSpace(_location))
            {
                Settings.Default.Route_StartLocation = _location;
                RaisePropertyChanged(() => Route_StartLocation);

                Logger.WriteDebug("Settings: " + $"found startpoint -> {Settings.Default.Route_StartLocation}");
            }
        }

        #endregion //Route 

        #region Fahrzeuge

        private RelayCommand addVehicleCommand;

        [Display(Description = "Öffnet den Dialog zum Hinzufügen eines neuen Fahrzeuges")]
        public RelayCommand AddVehicleCommand
        {
            get
            {
                if (addVehicleCommand == null)
                    addVehicleCommand = new RelayCommand(() => OnAddVehicle(), () => CanAddVehicle());

                return addVehicleCommand;
            }
        }

        private bool CanAddVehicle()
        {
            return true;
        }

        private void OnAddVehicle()
        {
            Logger.WriteDebug("Settings: AddVehicleCommand");

            Messenger.Default.Send(new SettingsVehicleDialog(business, 0));
        }

        private RelayCommand<object> editVehicleCommand;

        [Display(Description = "Öffnet den Dialog zum Hinzufügen eines neuen Fahrzeuges")]
        public RelayCommand<object> EditVehicleCommand
        {
            get
            {
                if (editVehicleCommand == null)
                    editVehicleCommand =
                        new RelayCommand<object>(param => OnEditVehicle(param), param => CanEditVehicle(param));

                return editVehicleCommand;
            }
        }

        private bool CanEditVehicle(object param)
        {
            if (param == null) return false;

            if (VehicleList == null || VehicleList.Count <= 0) return false;

            if (VehicleList.Where(c => c.Id == ((Vehicle) param).Id).SingleOrDefault() == null) return false;

            return true;
        }

        private void OnEditVehicle(object param)
        {
            Logger.WriteDebug("Settings: EditVehicleCommand");

            Messenger.Default.Send(new SettingsVehicleDialog(business, ((Vehicle) param).Id));
        }

        private RelayCommand<object> deleteVehicleCommand;

        [Display(Description = "Entfernt dieses Fahrzeuges")]
        public RelayCommand<object> DeleteVehicleCommand
        {
            get
            {
                if (deleteVehicleCommand == null)
                    deleteVehicleCommand = new RelayCommand<object>(param => OnDeleteVehicle(param),
                        param => CanDeleteVehicle(param));

                return deleteVehicleCommand;
            }
        }

        private bool CanDeleteVehicle(object param)
        {
            if (param == null) return false;

            if (VehicleList == null || VehicleList.Count <= 0) return false;

            if (VehicleList.Where(c => c.Id == ((Vehicle) param).Id).SingleOrDefault() == null) return false;

            return true;
        }

        private void OnDeleteVehicle(object param)
        {
            Logger.WriteDebug("Settings: DeleteVehicleCommand");

            business.DeleteVehicle((Vehicle) param);
            LoadVehicleList();
        }

        private RelayCommand importVehicleCommand;

        [Display(Description = "Auswahl der FMS32-Datei um alle Fahrzeuge daraus zu importieren.")]
        public RelayCommand ImportVehicleCommand
        {
            get
            {
                if (importVehicleCommand == null)
                    importVehicleCommand = new RelayCommand(() => OnImportVehicle(), () => CanImportVehicle());

                return importVehicleCommand;
            }
        }

        private bool CanImportVehicle()
        {
            return true;
        }

        private void OnImportVehicle()
        {
            try
            {
                Logger.WriteDebug("Settings: ImportVehicleCommand");

                //Show File Dialog
                var _openFileDialog = new OpenFileDialog();
                _openFileDialog.Filter = "FMS32-Datei | *.dat";
                _openFileDialog.RestoreDirectory = true;
                _openFileDialog.InitialDirectory = @"C:\Program Files (x86)\Heirue-Soft\FMS32-PRO";
                _openFileDialog.Title = "Wählen Sie die Importdatei(FAHRZEUG.DAT) aus:";
                if (_openFileDialog.ShowDialog() != DialogResult.OK) return;

                var _backgroundWorker = new BackgroundWorker();
                _backgroundWorker.DoWork += (sender, args) =>
                {
                    //Show wait screen
                    ServiceLocator.Current.GetInstance<WaitSplashScreen>().Show();

                    //Read text file   
                    var _importText = new List<string>();
                    using (var reader = new StreamReader(_openFileDialog.FileName, Encoding.Default))
                    {
                        var position = 0;
                        var buffer = new char[515];

                        while ((position = reader.ReadBlock(buffer, 0, 515)) != 0) _importText.Add(new string(buffer));
                    }

                    //Save in Database
                    foreach (var _data in _importText)
                    {
                        var _name = _data.Substring(8, 30).Replace("  ", "");
                        var _bosIdentifier = _data.Substring(0, 8).Replace("  ", "");
                        if (string.IsNullOrWhiteSpace(_name) || string.IsNullOrWhiteSpace(_bosIdentifier)) continue;

                        var _vehicle = new Vehicle
                        {
                            Name = _name,
                            BosIdentifier = _bosIdentifier
                        };
                        business.AddOrUpdateVehicle(_vehicle);
                    }
                };
                _backgroundWorker.RunWorkerCompleted += (sender, args) =>
                {
                    if (args.Error != null)
                    {
                        Logger.WriteError(MethodBase.GetCurrentMethod(), args.Error);
                        MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + args.Error.Message,
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    //Refresh List
                    LoadVehicleList();

                    ServiceLocator.Current.GetInstance<WaitSplashScreen>().Close();
                };
                // starts the background worker   
                _backgroundWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion //Fahrzeuge

        #region Pager

        private RelayCommand addPagerCommand;

        [Display(Description = "Öffnet den Dialog zum Hinzufügen eines neuen Pager")]
        public RelayCommand AddPagerCommand
        {
            get
            {
                if (addPagerCommand == null)
                    addPagerCommand = new RelayCommand(() => OnAddPager(), () => CanAddPager());

                return addPagerCommand;
            }
        }

        private bool CanAddPager()
        {
            return true;
        }

        private void OnAddPager()
        {
            try
            {
                Logger.WriteDebug("Settings: AddPagerCommand");

                Messenger.Default.Send(new SettingsPagerDialog(business, 0));
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand<object> editPagerCommand;

        [Display(Description = "Öffnet den Dialog zum bearbeiten eines Pagers")]
        public RelayCommand<object> EditPagerCommand
        {
            get
            {
                if (editPagerCommand == null)
                    editPagerCommand =
                        new RelayCommand<object>(param => OnEditPager(param), param => CanEditPager(param));

                return editPagerCommand;
            }
        }

        private bool CanEditPager(object param)
        {
            if (param == null) return false;

            if (PagerList == null || PagerList.Count <= 0) return false;

            if (PagerList.Where(c => c.Id == ((Pager) param).Id).SingleOrDefault() == null) return false;

            return true;
        }

        private void OnEditPager(object param)
        {
            try
            {
                Logger.WriteDebug("Settings: EditPagerCommand");

                Messenger.Default.Send(new SettingsPagerDialog(business, ((Pager) param).Id));
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand<object> deletePagerCommand;

        [Display(Description = "Entfernt diesen Pager")]
        public RelayCommand<object> DeletePagerCommand
        {
            get
            {
                if (deletePagerCommand == null)
                    deletePagerCommand =
                        new RelayCommand<object>(param => OnDeletePager(param), param => CanDeletePager(param));

                return deletePagerCommand;
            }
        }

        private bool CanDeletePager(object param)
        {
            if (param == null) return false;

            if (PagerList == null || PagerList.Count <= 0) return false;

            if (PagerList.Where(c => c.Id == ((Pager) param).Id).SingleOrDefault() == null) return false;

            return true;
        }

        private void OnDeletePager(object param)
        {
            try
            {
                Logger.WriteDebug("Settings: DeletePagerCommand");

                business.DeletePager((Pager) param);
                LoadPagerList();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand importPagerCommand;

        [Display(Description = "Auswahl der FMS32-Datei um alle Pager daraus zu importieren.")]
        public RelayCommand ImportPagerCommand
        {
            get
            {
                if (importPagerCommand == null)
                    importPagerCommand = new RelayCommand(() => OnImportPager(), () => CanImportPager());

                return importPagerCommand;
            }
        }

        private bool CanImportPager()
        {
            return true;
        }

        private void OnImportPager()
        {
            try
            {
                Logger.WriteDebug("Settings: ImportPagerCommand");

                //Show File Dialog
                var _openFileDialog = new OpenFileDialog();
                _openFileDialog.Filter = "FMS32-Datei | *.dat";
                _openFileDialog.RestoreDirectory = true;
                _openFileDialog.InitialDirectory = @"C:\Program Files (x86)\Heirue-Soft\FMS32-PRO";
                _openFileDialog.Title = "Wählen Sie die Importdatei(TON5.DAT) aus:";
                if (_openFileDialog.ShowDialog() != DialogResult.OK) return;

                var _backgroundWorker = new BackgroundWorker();
                _backgroundWorker.DoWork += (sender, args) =>
                {
                    //Show wait screen
                    ServiceLocator.Current.GetInstance<WaitSplashScreen>().Show();

                    //Read text file
                    var _importText = new List<string>();
                    using (var reader = new StreamReader(_openFileDialog.FileName, Encoding.Default))
                    {
                        var position = 0;
                        var buffer = new char[323];

                        while ((position = reader.ReadBlock(buffer, 0, 323)) != 0) _importText.Add(new string(buffer));
                    }

                    //Save in Database
                    foreach (var _data in _importText)
                    {
                        var _name = _data.Substring(5, 44).Replace("  ", "");
                        var _identifier = _data.Substring(0, 5).Replace("  ", "");
                        if (string.IsNullOrWhiteSpace(_name) || string.IsNullOrWhiteSpace(_identifier)) continue;

                        var _pager = new Pager
                        {
                            Identifier = _identifier,
                            Name = _name
                        };
                        business.AddOrUpdatePager(_pager);
                    }
                };
                _backgroundWorker.RunWorkerCompleted += (sender, args) =>
                {
                    if (args.Error != null)
                    {
                        Logger.WriteError(MethodBase.GetCurrentMethod(), args.Error);
                        MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + args.Error.Message,
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    //Refresh List
                    LoadPagerList();

                    ServiceLocator.Current.GetInstance<WaitSplashScreen>().Close();
                };
                // starts the background worker   
                _backgroundWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion //Pager    

        #endregion //Commands

        #region Events

        #endregion //Events

        #region Validation

        #endregion //Validation

        #region Public Properties

        #region Allgemein

        [Display(Description = "Schriftfarbe aller Texte.")]
        public Color ForegroundColor
        {
            get => Settings.Default.ForegroundColor;
            set
            {
                if (Settings.Default.ForegroundColor == value) return;

                Settings.Default.ForegroundColor = value;

                RaisePropertyChanged(() => ForegroundColor);
            }
        }

        [Display(Description = "Dieser Text läuft als Laufschrift im Hauptfenster von rechts nach links.")]
        public string Ticker_Text
        {
            get
            {
                if (Settings.Default.Ticker_IsFile) return "Dateisynchronisierung: " + Settings.Default.Ticker_Text;

                return Settings.Default.Ticker_Text;
            }
            set
            {
                if (Settings.Default.Ticker_Text == value) return;

                Settings.Default.Ticker_IsFile = false;
                Settings.Default.Ticker_Text = value;

                RaisePropertyChanged(() => Ticker_Text);
            }
        }

        [Display(Description = "Geschwindigkeit der Laufschrift im Hauptfenster.")]
        public double Ticker_Speed
        {
            get => Settings.Default.Ticker_Speed;
            set
            {
                if (Settings.Default.Ticker_Speed == value) return;

                Settings.Default.Ticker_Speed = (int) value;

                RaisePropertyChanged(() => Ticker_Speed);
            }
        }

        [Display(Description = "Neue Meldungen werden immer an der ersten Position der Liste eingefügt.")]
        public bool Pagers_InsertItem
        {
            get => Settings.Default.Pagers_InsertItem;
            set
            {
                if (Settings.Default.Pagers_InsertItem == value) return;

                Settings.Default.Pagers_InsertItem = value;

                RaisePropertyChanged(() => Pagers_InsertItem);
            }
        }

        public Array Riverlevel_ModeList => Enum.GetValues(typeof(RiverlevelMode));

        public RiverlevelMode Riverlevel_Mode
        {
            get => Settings.Default.Riverlevel_Mode;
            set
            {
                if (Settings.Default.Riverlevel_Mode == value) return;

                Settings.Default.Riverlevel_Mode = value;

                RaisePropertyChanged(() => Riverlevel_Mode);
            }
        }

        [Display(Description =
            "Messstellen-Nummer des Hochwassernachrichtendienst(http://hnd.bayern.de/)\nZu finden unter Stammdaten der Messtselle")]
        public string Riverlevel_Messstelle
        {
            get => Settings.Default.Riverlevel_Messstelle;
            set
            {
                if (Settings.Default.Riverlevel_Messstelle == value) return;

                Settings.Default.Riverlevel_Messstelle = value;

                RaisePropertyChanged(() => Riverlevel_Messstelle);
            }
        }

        [Display(Description =
            "Adresse die im Webbrowserfenster angezeigt wird. Die Seite wird jede Minute aktualisiert")]
        public string Warnweather_Url
        {
            get => Settings.Default.Warnweather_Url;
            set
            {
                if (Settings.Default.Warnweather_Url == value) return;

                Settings.Default.Warnweather_Url = value;

                RaisePropertyChanged(() => Warnweather_Url);
            }
        }

        [Display(Description =
            "Adresse die im Webbrowserfenster angezeigt wird. Die Seite wird jede Minute aktualisiert")]
        public string Webbrowser_Url
        {
            get => Settings.Default.Webbrowser_Url;
            set
            {
                if (Settings.Default.Webbrowser_Url == value) return;

                Settings.Default.Webbrowser_Url = value;

                RaisePropertyChanged(() => Webbrowser_Url);
            }
        }

        [Display(Description = "Die Einsatzmittel werden mit der Statusbox und Status als Text angezeigt")]
        public bool Vehicles_Statusbox
        {
            get => Settings.Default.Vehicles_Statusbox;
            set
            {
                if (Settings.Default.Vehicles_Statusbox == value) return;

                Settings.Default.Vehicles_Statusbox = value;

                RaisePropertyChanged(() => Vehicles_Statusbox);
            }
        }

        [Display(Description =
            "Erhält ein Fahrzeug bei dem 'Auf Fax' aktiviert ist, einen Status der für das Alarmfenster aktiviert ist, wird es automatisch zu den Einsatzmitteln im Alarmfenster hinzugefügt.")]
        public bool Vehicles_AutoDisponieren
        {
            get => Settings.Default.Vehicles_AutoDisponieren;
            set
            {
                if (Settings.Default.Vehicles_AutoDisponieren == value) return;

                Settings.Default.Vehicles_AutoDisponieren = value;

                RaisePropertyChanged(() => Vehicles_AutoDisponieren);
            }
        }

        [Display(Description =
            "Führt die bei einem Fahrzeug hinterlegte Datei bei einem Status C für das Fahrzeug aus. Ansonsten wird die Datei ausgeführt wenn das Fahrzeug als Einsatzmittel auf dem Fax steht.")]
        public bool AppExecute_OnStatus
        {
            get => Settings.Default.AppExecute_OnStatus;
            set
            {
                if (Settings.Default.AppExecute_OnStatus == value) return;

                Settings.Default.AppExecute_OnStatus = value;

                RaisePropertyChanged(() => AppExecute_OnStatus);
            }
        }

        [Display(Description =
            "Das System wird automatisch um Mitternacht neugestartet, falls kein Alarmfenster offen ist. Achtung das RIS muss dazu auch im Ordner Autostart liegen!")]
        public bool Reboot_On
        {
            get => Settings.Default.Reboot_On;
            set
            {
                if (Settings.Default.Reboot_On == value) return;

                Settings.Default.Reboot_On = value;

                RaisePropertyChanged(() => Reboot_On);
            }
        }

        #endregion //Allgemein

        #region Monitor

        [Display(Description = "Anzeigezeit des Alarmfensters")]
        public TimeSpan Monitor_AlarmTime
        {
            get => Settings.Default.Monitor_AlarmTime;
            set
            {
                if (Settings.Default.Monitor_AlarmTime == value) return;

                Settings.Default.Monitor_AlarmTime = value;

                RaisePropertyChanged(() => Monitor_AlarmTime);
            }
        }

        [Display(Description = "Verzögerung nach dem schließen des Alarmfenster, bevor Monitor in Standby wechselt")]
        public TimeSpan Monitor_AlarmDelayTime
        {
            get => Settings.Default.Monitor_AlarmDelayTime;
            set
            {
                if (Settings.Default.Monitor_AlarmDelayTime == value) return;

                Settings.Default.Monitor_AlarmDelayTime = value;

                RaisePropertyChanged(() => Monitor_AlarmDelayTime);
            }
        }

        [Display(Description = "Aufwachen des Monitors bei einem FMS-Status eines eigenen Fahrzeuges")]
        public TimeSpan Monitor_WakeupStatusTime
        {
            get => Settings.Default.Monitor_WakeupStatusTime;
            set
            {
                if (Settings.Default.Monitor_WakeupStatusTime == value) return;

                Settings.Default.Monitor_WakeupStatusTime = value;

                RaisePropertyChanged(() => Monitor_WakeupStatusTime);
            }
        }

        [Display(Description = "Aufwachen des Monitors bei einer Pager-Alarmierung")]
        public TimeSpan Monitor_WakeupAlarmTime
        {
            get => Settings.Default.Monitor_WakeupAlarmTime;
            set
            {
                if (Settings.Default.Monitor_WakeupAlarmTime == value) return;

                Settings.Default.Monitor_WakeupAlarmTime = value;

                RaisePropertyChanged(() => Monitor_WakeupAlarmTime);
            }
        }

        [Display(Description =
            "Deaktiviert den wöchentlichen Monitor-Zeitplan. Der Bildschirm bleibt immer eingeschaltet.")]
        public bool Monitor_WeekplanDisabled
        {
            get => Settings.Default.Monitor_WeekplanDisabled;
            set
            {
                if (Settings.Default.Monitor_WeekplanDisabled == value) return;

                Settings.Default.Monitor_WeekplanDisabled = value;

                RaisePropertyChanged(() => Monitor_WeekplanDisabled);
            }
        }

        private SettingsMonitor monitorSettings;

        public SettingsMonitor MonitorSettings
        {
            get => monitorSettings;
            set
            {
                if (monitorSettings == value) return;

                monitorSettings = value;

                RaisePropertyChanged(() => MonitorSettings);
            }
        }

        #endregion //Monitor

        #region Route
        
        [Display(Description = "Koordinaten des Routen-Startpunkts")]
        public string Route_StartLocation => Settings.Default.Route_StartLocation;

        [Display(Description = "Postleitzahl des Routen-Startpunkts")]
        public string Route_Zip
        {
            get => Settings.Default.Route_Zip;
            set
            {
                if (Settings.Default.Route_Zip == value) return;

                Settings.Default.Route_Zip = value;

                RaisePropertyChanged(() => Route_Zip);
            }
        }

        [Display(Description = "Ort des Routen-Startpunkts")]
        public string Route_City
        {
            get => Settings.Default.Route_City;
            set
            {
                if (Settings.Default.Route_City == value) return;

                Settings.Default.Route_City = value;

                RaisePropertyChanged(() => Route_City);
            }
        }

        [Display(Description = "Straße des Routen-Startpunkts")]
        public string Route_Street
        {
            get => Settings.Default.Route_Street;
            set
            {
                if (Settings.Default.Route_Street == value) return;

                Settings.Default.Route_Street = value;

                RaisePropertyChanged(() => Route_Street);
            }
        }

        [Display(Description = "Hausnummer des Routen-Startpunkts")]
        public string Route_Housenumber
        {
            get => Settings.Default.Route_Housenumber;
            set
            {
                if (Settings.Default.Route_Housenumber == value) return;

                Settings.Default.Route_Housenumber = value;

                RaisePropertyChanged(() => Route_Housenumber);
            }
        }

        [Display(Description = "Es werden nur Routen mit hoher Treffergenauigkeit der Hausnummer verwendet")]
        public bool Route_HighQuality
        {
            get => Settings.Default.Route_HighQuality;
            set
            {
                if (Settings.Default.Route_HighQuality == value) return;

                Settings.Default.Route_HighQuality = value;

                RaisePropertyChanged(() => Route_HighQuality);
            }
        }

        [Display(Description = "Verwenden zur Planung die schnellste Route, ansonsten die Kürzeste")]
        public bool Route_ModeFastest
        {
            get => Settings.Default.Route_ModeFastest;
            set
            {
                if (Settings.Default.Route_ModeFastest == value) return;

                Settings.Default.Route_ModeFastest = value;

                RaisePropertyChanged(() => Route_ModeFastest);
            }
        }

        [Display(Description = "Verwenden zur Planung nur für LKW zugelassene Straßen")]
        public bool Route_ModeTruck
        {
            get => Settings.Default.Route_ModeTruck;
            set
            {
                if (Settings.Default.Route_ModeTruck == value) return;

                Settings.Default.Route_ModeTruck = value;

                RaisePropertyChanged(() => Route_ModeTruck);
            }
        }

        [Display(Description = "Bezieht die aktuelle Verkehrslage mit in die Planung ein")]
        public bool Route_ModeTraffic
        {
            get => Settings.Default.Route_ModeTraffic;
            set
            {
                if (Settings.Default.Route_ModeTraffic == value) return;

                Settings.Default.Route_ModeTraffic = value;

                RaisePropertyChanged(() => Route_ModeTraffic);
            }
        }

        #endregion //Route

        #region Funkmeldesystem

        private SettingsFms fmsSettings;

        public SettingsFms FmsSettings
        {
            get => fmsSettings;
            set
            {
                if (fmsSettings == value) return;

                fmsSettings = value;

                RaisePropertyChanged(() => FmsSettings);
            }
        }

        #endregion //Funkmeldesystem

        #region Fahrzeuge

        private ObservableCollection<Vehicle> vehicleList;

        public ObservableCollection<Vehicle> VehicleList
        {
            get => vehicleList;
            set
            {
                if (vehicleList == value) return;

                vehicleList = value;

                RaisePropertyChanged(() => VehicleList);
            }
        }

        #endregion //Fahrzeuge     

        #region Pager

        private ObservableCollection<Pager> pagerList;

        public ObservableCollection<Pager> PagerList
        {
            get => pagerList;
            set
            {
                if (pagerList == value) return;

                pagerList = value;

                RaisePropertyChanged(() => PagerList);
            }
        }

        #endregion //Pager

        #endregion //Public Properties

        #region Private Properties

        #endregion //Private Properties

        #region Private Functions

        private void LoadVehicleList()
        {
            vehicleList = new ObservableCollection<Vehicle>(business.GetAllVehicleAsync().Result);
            RaisePropertyChanged(() => VehicleList);
        }

        private void LoadPagerList()
        {
            pagerList = new ObservableCollection<Pager>(business.GetAllPagerAsync().Result);
            RaisePropertyChanged(() => PagerList);
        }

        private void SaveMonitorSettings()
        {
            MonitorSettings.Monday =
                new ObservableCollection<SettingsMonitor.MonitorItem>(MonitorSettings.Monday.OrderBy(m => m.Start));
            MonitorSettings.Tuesday =
                new ObservableCollection<SettingsMonitor.MonitorItem>(MonitorSettings.Tuesday.OrderBy(m => m.Start));
            MonitorSettings.Wednesday =
                new ObservableCollection<SettingsMonitor.MonitorItem>(MonitorSettings.Wednesday.OrderBy(m => m.Start));
            MonitorSettings.Thursday =
                new ObservableCollection<SettingsMonitor.MonitorItem>(MonitorSettings.Thursday.OrderBy(m => m.Start));
            MonitorSettings.Friday =
                new ObservableCollection<SettingsMonitor.MonitorItem>(MonitorSettings.Friday.OrderBy(m => m.Start));
            MonitorSettings.Saturday =
                new ObservableCollection<SettingsMonitor.MonitorItem>(MonitorSettings.Saturday.OrderBy(m => m.Start));
            MonitorSettings.Sunday =
                new ObservableCollection<SettingsMonitor.MonitorItem>(MonitorSettings.Sunday.OrderBy(m => m.Start));
            RaisePropertyChanged(() => MonitorSettings);
        }

        #endregion //Private Funtions
    }
}