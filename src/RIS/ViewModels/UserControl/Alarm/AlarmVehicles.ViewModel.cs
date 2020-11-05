#region

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using RIS.Business;
using RIS.Core;
using RIS.Core.Decoder;
using RIS.Core.Fax;
using RIS.Model;
using RIS.Properties;
using SRS.Utilities;
using MessageBox = RIS.Core.Helper.MessageBox;

#endregion

namespace RIS.ViewModels
{
    public class AlarmVehiclesViewModel : ViewModelBase
    {
        private readonly IBusiness business;
        private readonly IDecoderService decoderService;
        private readonly string einsatzGuid;

        public AlarmVehiclesViewModel(Einsatz _einsatz)
        {
            try
            {
                einsatzGuid = _einsatz.Guid;

                business = ServiceLocator.Current.GetInstance<IBusiness>();

                decoderService = ServiceLocator.Current.GetInstance<IDecoderService>();
                if (decoderService != null)
                    decoderService.FmsMessageReceived += (sender, e) => DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        decoderService_MessageFmsReceived(sender, e);
                    });

                Vehicles = new ObservableCollection<VehicleViewModel>();
                ContextMenuVehicles = new ObservableCollection<Vehicle>(business.GetVehiclesAreEinsatzmittel());
                foreach (var _vehicle in _einsatz.Einsatzmittel)
                {
                    //Add vehicle to view
                    var _alarmVehicle = addVehicle(_vehicle);
                    if (_alarmVehicle == null) continue;

                    //Transfer Status from main view 
                    var _mainVehicle = ServiceLocator.Current.GetInstance<MainVehiclesViewModel>().Vehicles
                        .Where(v => v.Vehicle.Id == _alarmVehicle.Vehicle.Id).FirstOrDefault();
                    if (_mainVehicle != null) _alarmVehicle.ChangeStatus(_mainVehicle.StatusText);

                    //Remove vehicle from ContextMenu if already in alarm list
                    var _contextMenuVehicle = ContextMenuVehicles.Where(c => c.Id == _vehicle.Id).FirstOrDefault();
                    if (_contextMenuVehicle != null) ContextMenuVehicles.Remove(_contextMenuVehicle);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public override void Cleanup()
        {
            if (decoderService != null)
                decoderService.FmsMessageReceived -= (sender, e) => DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    decoderService_MessageFmsReceived(sender, e);
                });

            base.Cleanup();
        }

        #region Commands

        private RelayCommand<object> addVehicleCommand;

        public RelayCommand<object> AddVehicleCommand
        {
            get
            {
                if (addVehicleCommand == null)
                    addVehicleCommand =
                        new RelayCommand<object>(param => OnAddVehicle(param), param => CanAddVehicle(param));

                return addVehicleCommand;
            }
        }

        private bool CanAddVehicle(object param)
        {
            if (param == null) return false;

            if (Vehicles == null) return false;

            return true;
        }

        private void OnAddVehicle(object param)
        {
            try
            {
                var _vehicle = param as Vehicle;
                if (_vehicle == null) return;

                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "AddVehicleCommand -> " + _vehicle.Name);

                //Add vehicle to view
                var _alarmVehicle = addVehicle(_vehicle);
                if (_alarmVehicle == null) return;

                //Transfer Status from main view 
                var _mainVehicle = ServiceLocator.Current.GetInstance<MainVehiclesViewModel>().Vehicles
                    .Where(v => v.Vehicle.Id == _alarmVehicle.Vehicle.Id).FirstOrDefault();
                if (_mainVehicle != null) _alarmVehicle.ChangeStatus(_mainVehicle.StatusText);

                //Remove vehicle from ContextMenu if already in alarm list
                var _contextMenuVehicle = ContextMenuVehicles.Where(c => c.Id == _vehicle.Id).FirstOrDefault();
                if (_contextMenuVehicle != null) ContextMenuVehicles.Remove(_contextMenuVehicle);
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

        private int rowCount;

        public int RowCount
        {
            get => rowCount;
            set
            {
                if (rowCount == value) return;

                rowCount = value;

                RaisePropertyChanged(() => RowCount);
            }
        }

        private int columnCount = 1;

        public int ColumnCount
        {
            get => columnCount;
            set
            {
                if (columnCount == value) return;

                columnCount = value;

                RaisePropertyChanged(() => ColumnCount);
            }
        }

        private ObservableCollection<VehicleViewModel> vehicles;

        public ObservableCollection<VehicleViewModel> Vehicles
        {
            get => vehicles;
            set
            {
                if (vehicles == value) return;

                vehicles = value;

                RaisePropertyChanged(() => Vehicles);
            }
        }

        private ObservableCollection<Vehicle> contextMenuVehicles;

        public ObservableCollection<Vehicle> ContextMenuVehicles
        {
            get => contextMenuVehicles;
            set
            {
                if (contextMenuVehicles == value) return;

                contextMenuVehicles = value;

                RaisePropertyChanged(() => Vehicles);
            }
        }

        #endregion //Public Properties

        #region Public Functions

        #endregion //Public Functions

        #region Private Properties

        private int currentRow;
        private int currentColumn;

        #endregion //Private Properties

        #region Private Functions

        public VehicleViewModel addVehicle(Vehicle _vehicle)
        {
            if (_vehicle == null || string.IsNullOrWhiteSpace(_vehicle.ViewText)) return null;

            //Increase column count if row is 8
            if (currentRow >= 8)
            {
                currentRow = 0;
                currentColumn++;
                ColumnCount++;
            }

            //Create vehicle and add to collection
            var _vehicleVM = new VehicleViewModel(_vehicle, currentRow, currentColumn, true);
            Vehicles.Add(_vehicleVM);
            RaisePropertyChanged(() => Vehicles);

            //Increase row position for next vehicle
            currentRow++;

            //Increase row count until 8
            if (RowCount < 8) RowCount++;

            return _vehicleVM;
        }

        private void decoderService_MessageFmsReceived(object sender, FmsMessageEventArgs e)
        {
            if (e == null || e.Vehicle == null) return;

            //Get vehicle from list
            var _vehicleViewModel = Vehicles.Where(v => v.Vehicle.Id == e.Vehicle.Id).FirstOrDefault();

            //Auto-Disponieren
            if (_vehicleViewModel == null && Settings.Default.Vehicles_AutoDisponieren &&
                !string.IsNullOrWhiteSpace(e.Vehicle.FaxText) &&
                (e.Status == "3" || e.Status == "4" || e.Status == "C"))
                _vehicleViewModel = addVehicle(e.Vehicle);

            //Change status
            if (_vehicleViewModel != null) _vehicleViewModel.ChangeStatus(e.Status);
        }

        #endregion //Private Funtions
    }
}