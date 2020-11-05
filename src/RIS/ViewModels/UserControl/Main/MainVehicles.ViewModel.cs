#region

using System;
using System.Collections.ObjectModel;
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
using RIS.Core.Decoder;
using RIS.Properties;
using RIS.Views;
using SRS.Utilities;
using MessageBox = RIS.Core.Helper.MessageBox;

#endregion

namespace RIS.ViewModels
{
    public class MainVehiclesViewModel : ViewModelBase
    {
        private readonly IBusiness business;
        private readonly IDecoderService decoderService;

        public MainVehiclesViewModel()
        {
            try
            {
                business = ServiceLocator.Current.GetInstance<IBusiness>();

                decoderService = ServiceLocator.Current.GetInstance<IDecoderService>();
                if (decoderService != null)
                    decoderService.FmsMessageReceived += (sender, e) => DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        decoderService_MessageFmsReceived(sender, e);
                    });

                //Set columns and rows
                RowCount = Settings.Default.Vehicles_Rows;
                ColumnCount = Settings.Default.Vehicles_Columns;
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        ~MainVehiclesViewModel()
        {
            try
            {
                if (decoderService != null)
                    decoderService.FmsMessageReceived -= (sender, e) => DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        decoderService_MessageFmsReceived(sender, e);
                    });

                //Save grid 
                Settings.Default.Vehicles_Rows = RowCount;
                Settings.Default.Vehicles_Columns = ColumnCount;
                Settings.Default.Save();

                //Save current status
                if (Vehicles != null)
                {
                    var _stringBuilder = new StringBuilder();
                    foreach (var _vehicle in Vehicles)
                        _stringBuilder.AppendLine(_vehicle.Vehicle.Id + "|" + _vehicle.StatusText);

                    File.WriteAllText(App.Path_DataVehicles, _stringBuilder.ToString());
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #region Private Functions

        private void decoderService_MessageFmsReceived(object sender, FmsMessageEventArgs e)
        {
            if (e == null || e.Vehicle == null) return;

            var _vehicleViewModel = Vehicles.Where(v => v.Vehicle.Id == e.Vehicle.Id).FirstOrDefault();
            if (_vehicleViewModel == null) return;

            _vehicleViewModel.ChangeStatus(e.Status);
        }

        #endregion //Private Funtions

        #region Commands

        private RelayCommand vehicleChangeCommand;

        public RelayCommand VehicleChangeCommand
        {
            get
            {
                if (vehicleChangeCommand == null)
                    vehicleChangeCommand = new RelayCommand(() => OnVehicleChange(), () => CanVehicleChange());

                return vehicleChangeCommand;
            }
        }

        private bool CanVehicleChange()
        {
            if (MouseRow == null || MouseColumn == null) return false;

            return true;
        }

        private void OnVehicleChange()
        {
            try
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "VehicleChangeCommand");

                var _vehicleDialog = new MainVehicleWindow(MouseRow.Value, MouseColumn.Value);
                _vehicleDialog.ShowDialog();

                Initialize();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand vehicleDeleteCommand;

        public RelayCommand VehicleDeleteCommand
        {
            get
            {
                if (vehicleDeleteCommand == null)
                    vehicleDeleteCommand = new RelayCommand(() => OnVehicleDelete(), () => CanVehicleDelete());

                return vehicleDeleteCommand;
            }
        }

        private bool CanVehicleDelete()
        {
            if (MouseRow == null || MouseColumn == null) return false;

            return true;
        }

        private void OnVehicleDelete()
        {
            try
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "VehicleDeleteCommand");

                business.RemoveVehicleByPosition(MouseRow.Value, MouseColumn.Value);
                Initialize();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand rowAddCommand;

        public RelayCommand RowAddCommand
        {
            get
            {
                if (rowAddCommand == null) rowAddCommand = new RelayCommand(() => OnRowAdd(), () => CanRowAdd());

                return rowAddCommand;
            }
        }

        private bool CanRowAdd()
        {
            return true;
        }

        private void OnRowAdd()
        {
            try
            {
                RowCount++;
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand rowDeleteCommand;

        public RelayCommand RowDeleteCommand
        {
            get
            {
                if (rowDeleteCommand == null)
                    rowDeleteCommand = new RelayCommand(() => OnRowDelete(), () => CanRowDelete());

                return rowDeleteCommand;
            }
        }

        private bool CanRowDelete()
        {
            if (RowCount <= 1) return false;

            return true;
        }

        private void OnRowDelete()
        {
            try
            {
                RowCount--;
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand columnAddCommand;

        public RelayCommand ColumnAddCommand
        {
            get
            {
                if (columnAddCommand == null)
                    columnAddCommand = new RelayCommand(() => OnColumnAdd(), () => CanColumnAdd());

                return columnAddCommand;
            }
        }

        private bool CanColumnAdd()
        {
            return true;
        }

        private void OnColumnAdd()
        {
            try
            {
                ColumnCount++;
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand columnDeleteCommand;

        public RelayCommand ColumnDeleteCommand
        {
            get
            {
                if (columnDeleteCommand == null)
                    columnDeleteCommand = new RelayCommand(() => OnColumnDelete(), () => CanColumnDelete());

                return columnDeleteCommand;
            }
        }

        private bool CanColumnDelete()
        {
            if (ColumnCount <= 1) return false;

            return true;
        }

        private void OnColumnDelete()
        {
            try
            {
                ColumnCount--;
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

        public int? MouseRow { get; set; }
        public int? MouseColumn { get; set; }

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

        private int columnCount;

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

        #endregion //Public Properties

        #region Public Functions

        public void Initialize()
        {
            var _vehiclesOnMain = business.GetVehiclesAreMain();
            if (_vehiclesOnMain == null) return;

            var _tempVehicles = Vehicles;
            Vehicles = new ObservableCollection<VehicleViewModel>();
            foreach (var _vehicle in _vehiclesOnMain)
            {
                var _vehicleViewModel =
                    new VehicleViewModel(_vehicle, _vehicle.MainRow.Value, _vehicle.MainColumn.Value, false);
                _vehicleViewModel.ChangeStatus("2");
                Vehicles.Add(_vehicleViewModel);

                if (_tempVehicles == null) continue;

                var _tempVehicle = _tempVehicles.Where(v => v.Vehicle.Id == _vehicle.Id).FirstOrDefault();
                if (_tempVehicle != null) _vehicleViewModel.ChangeStatus(_tempVehicle.StatusText);
            }

            RaisePropertyChanged(() => Vehicles);

            //Load Status from file 
            if (File.Exists(App.Path_DataVehicles))
                foreach (var line in File.ReadLines(App.Path_DataVehicles))
                {
                    var _item = line.Split('|');

                    var _vehicle =
                        Vehicles.Where(v => v.Vehicle.Id == Convert.ToInt32(_item[0])).FirstOrDefault();
                    if (_vehicle == null) continue;

                    _vehicle.ChangeStatus(_item[1]);
                }
        }

        public void Reset()
        {
            if (Vehicles == null || Vehicles.Count == 0) return;

            //Change Status
            foreach (var _vehicleViewModel in Vehicles) _vehicleViewModel.ChangeStatus("2");

            //Save current status
            var _stringBuilder = new StringBuilder();
            foreach (var _vehicle in Vehicles)
                _stringBuilder.AppendLine(_vehicle.Vehicle.Id + "|" + _vehicle.StatusText);

            File.WriteAllText(App.Path_DataVehicles, _stringBuilder.ToString());
        }

        #endregion //Public Functions

        #region Private Properties

        #endregion //Private Properties
    }
}