#region

using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using RIS.Business;
using RIS.Model;
using RIS.Views;
using SRS.Utilities;
using MessageBox = RIS.Core.Helper.MessageBox;

#endregion

namespace RIS.ViewModels
{
    public class SettingsAlarmappViewModel : ViewModelBase
    {
        private readonly AlarmappGroup alarmappGroup;
        private readonly IBusiness business;

        public SettingsAlarmappViewModel(IBusiness _business, int _id)
        {
            try
            {
                business = _business;

                //Query lists for selection            
                VehicleList = new ObservableCollection<Vehicle>(business.GetAllVehicleAsync().Result);
                PagerList = new ObservableCollection<Pager>(business.GetAllPagerAsync().Result);

                //Query item with relations
                alarmappGroup = business.GetAlarmappGroupById(_id);
                if (alarmappGroup == null) alarmappGroup = new AlarmappGroup();

                //Do selection from list
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
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

            if (alarmappGroup.Vehicles == null) return false;

            if (alarmappGroup.Vehicles.Where(a => a.Id == ((Vehicle) param).Id).SingleOrDefault() != null) return false;

            return true;
        }

        private void OnAddVehicle(object param)
        {
            alarmappGroup.Vehicles.Add((Vehicle) param);
            RaisePropertyChanged(() => AlarmappGroupVehicleList);
        }

        private RelayCommand<object> removeVehicleCommand;

        public RelayCommand<object> RemoveVehicleCommand
        {
            get
            {
                if (removeVehicleCommand == null)
                    removeVehicleCommand = new RelayCommand<object>(param => OnRemoveVehicle(param),
                        param => CanRemoveVehicle(param));

                return removeVehicleCommand;
            }
        }

        private bool CanRemoveVehicle(object param)
        {
            if (param == null) return false;

            if (alarmappGroup.Vehicles == null) return false;

            if (alarmappGroup.Vehicles.Where(a => a.Id == ((Vehicle) param).Id).SingleOrDefault() == null) return false;

            return true;
        }

        private void OnRemoveVehicle(object param)
        {
            alarmappGroup.Vehicles.Remove((Vehicle) param);
            RaisePropertyChanged(() => AlarmappGroupVehicleList);
        }

        private RelayCommand<object> addPagerCommand;

        public RelayCommand<object> AddPagerCommand
        {
            get
            {
                if (addPagerCommand == null)
                    addPagerCommand = new RelayCommand<object>(param => OnAddPager(param), param => CanAddPager(param));

                return addPagerCommand;
            }
        }

        private bool CanAddPager(object param)
        {
            if (param == null) return false;

            if (alarmappGroup.Pagers == null) return false;

            if (alarmappGroup.Pagers.Where(a => a.Id == ((Pager) param).Id).SingleOrDefault() != null) return false;

            return true;
        }

        private void OnAddPager(object param)
        {
            alarmappGroup.Pagers.Add((Pager) param);
            RaisePropertyChanged(() => AlarmappGroupPagerList);
        }

        private RelayCommand<object> removePagerCommand;

        public RelayCommand<object> RemovePagerCommand
        {
            get
            {
                if (removePagerCommand == null)
                    removePagerCommand =
                        new RelayCommand<object>(param => OnRemovePager(param), param => CanRemovePager(param));

                return removePagerCommand;
            }
        }

        private bool CanRemovePager(object param)
        {
            if (param == null) return false;

            if (alarmappGroup.Pagers == null) return false;

            if (alarmappGroup.Pagers.Where(a => a.Id == ((Pager) param).Id).SingleOrDefault() == null) return false;

            return true;
        }

        private void OnRemovePager(object param)
        {
            alarmappGroup.Pagers.Remove((Pager) param);
            RaisePropertyChanged(() => AlarmappGroupPagerList);
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
                business.AddOrUpdateAlarmappGroup(alarmappGroup);

                OnClose();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

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
            Messenger.Default.Send<SettingsAlarmappDialog>(null);
        }

        #endregion //Commands

        #region Events

        #endregion //Events

        #region Validation

        #endregion //Validation

        #region Public Properties

        public string WindowTitel => "AlarmApp";

        public string GroupName => alarmappGroup.GroupName;

        [Display(Description =
            "Faxinformation werden an die App weitergeleitet.\nBei ausgewählten Fahrzeugen, wird die Gruppe nur alarmiert wenn das Fahrzeug auch als Einsatzmittel alarmiert ist.")]
        public bool FaxOn
        {
            get => alarmappGroup.FaxOn;
            set
            {
                if (alarmappGroup.FaxOn == value) return;

                alarmappGroup.FaxOn = value;

                RaisePropertyChanged(() => FaxOn);
            }
        }

        [Display(Description = "Faxinformationen werden nur bei vorangegangenen Pageralarm weitergeleitet.")]
        public bool OnlyWithPagerOn
        {
            get => alarmappGroup.OnlyWithPager;
            set
            {
                if (alarmappGroup.OnlyWithPager == value) return;

                alarmappGroup.OnlyWithPager = value;

                RaisePropertyChanged(() => OnlyWithPagerOn);
            }
        }

        public ObservableCollection<Pager> PagerList { get; }

        public ObservableCollection<Pager> AlarmappGroupPagerList
        {
            get { return new ObservableCollection<Pager>(alarmappGroup.Pagers.OrderBy(z => z.Identifier)); }
        }

        public ObservableCollection<Vehicle> VehicleList { get; }

        public ObservableCollection<Vehicle> AlarmappGroupVehicleList
        {
            get { return new ObservableCollection<Vehicle>(alarmappGroup.Vehicles.OrderBy(z => z.Name)); }
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