#region

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using RIS.Business;
using RIS.Model;
using SRS.Utilities;
using MessageBox = RIS.Core.Helper.MessageBox;

#endregion

namespace RIS.ViewModels
{
    public class MainVehicleViewModel : ViewModelBase
    {
        private readonly IBusiness business;
        private readonly int column;
        private readonly int row;
        private readonly Vehicle vehicle;

        public MainVehicleViewModel(int _row, int _column)
        {
            try
            {
                business = ServiceLocator.Current.GetInstance<IBusiness>();

                row = _row;
                column = _column;

                vehicles = new ObservableCollection<Vehicle>(business.GetAllVehicleAsync().Result);
                vehicle = business.GetVehicleByPosition(_row, _column);
                if (vehicle != null) SelectedVehicle = Vehicles.Where(v => v.Id == vehicle.Id).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #region Commands

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
            if (SelectedVehicle == null) return false;

            return true;
        }

        private void OnSave()
        {
            try
            {
                //If selected remove current vehicle    
                business.RemoveVehicleByPosition(row, column);

                SelectedVehicle.MainRow = row;
                SelectedVehicle.MainColumn = column;
                business.AddOrUpdateVehicle(SelectedVehicle);

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

        public string WindowTitel => "Fahrzeug";

        private ObservableCollection<Vehicle> vehicles;

        public ObservableCollection<Vehicle> Vehicles
        {
            get => vehicles;
            set
            {
                if (vehicles == value) return;

                vehicles = value;

                RaisePropertyChanged(() => Vehicles);
            }
        }

        private Vehicle selectedVehicle;

        public Vehicle SelectedVehicle
        {
            get => selectedVehicle;
            set
            {
                if (selectedVehicle == value) return;

                selectedVehicle = value;

                RaisePropertyChanged(() => SelectedVehicle);
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