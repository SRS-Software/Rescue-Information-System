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
    public class SettingsAaoViewModel : ViewModelBase
    {
        private readonly Aao aao;
        private readonly IBusiness business;

        public SettingsAaoViewModel(IBusiness _business, int _id)
        {
            try
            {
                business = _business;

                //Query lists for selection
                ConditionList = new ObservableCollection<AaoCondition>(business.GetAllAaoConditionAsync().Result);
                CombinationList = new ObservableCollection<Aao>(business.GetAllAaoAsync().Result);
                VehicleList = new ObservableCollection<Vehicle>(business.GetAllVehicleAsync().Result);

                //Query item with relations
                aao = business.GetAaoById(_id);
                if (aao == null) aao = new Aao();

                //Do selection from list
                if (aao.Condition != null)
                    SelectedCondition = ConditionList.Where(c => c.Id == aao.Condition.Id).SingleOrDefault();

                if (aao.Combination != null)
                    SelectedCombination = CombinationList.Where(c => c.Id == aao.Combination.Id).SingleOrDefault();
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

            if (aao.Vehicles == null) return false;

            if (aao.Vehicles.Where(a => a.Vehicle.Id == ((Vehicle) param).Id).SingleOrDefault() != null) return false;

            return true;
        }

        private void OnAddVehicle(object param)
        {
            //Add new vehicles to list
            aao.Vehicles.Add(new AaoVehicle
            {
                Position = aao.Vehicles.Count,
                Vehicle = (Vehicle) param
            });

            RaisePropertyChanged(() => AaoVehicleList);
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

            if (aao.Vehicles == null) return false;

            if (aao.Vehicles.Where(a => a.Vehicle.Id == ((AaoVehicle) param).Vehicle.Id).SingleOrDefault() ==
                null) return false;

            return true;
        }

        private void OnRemoveVehicle(object param)
        {
            try
            {
                //Remove vehicle from list
                aao.Vehicles.Remove((AaoVehicle) param);

                var position = 0;
                foreach (var vehicle in aao.Vehicles)
                {
                    vehicle.Position = position;
                    position++;
                }

                RaisePropertyChanged(() => AaoVehicleList);
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
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
            if (aao == null) return false;

            if (string.IsNullOrWhiteSpace(aao.Name)) return false;

            if (aao.Condition == null) return false;

            if (string.IsNullOrWhiteSpace(aao.Expression)) return false;

            return true;
        }

        private void OnSave()
        {
            try
            {
                business.AddOrUpdateAaoAsync(aao);

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
            Messenger.Default.Send<SettingsAaoDialog>(null);
        }

        #endregion //Commands

        #region Events

        #endregion //Events

        #region Validation

        #endregion //Validation

        #region Public Properties

        public string WindowTitel => "Alarm- und Ausrückeordnung";

        [Display(Description = "Beschreibung der AAO")]
        public string Name
        {
            get => aao.Name;
            set
            {
                if (aao.Name == value) return;

                aao.Name = value;

                RaisePropertyChanged(() => Name);
            }
        }

        public ObservableCollection<AaoCondition> ConditionList { get; }

        [Display(Description = "Bedingung der AAO")]
        public AaoCondition SelectedCondition
        {
            get => aao.Condition;
            set
            {
                if (aao.Condition == value) return;

                aao.Condition = value;

                RaisePropertyChanged(() => SelectedCondition);
            }
        }

        [Display(Description = "Wert den die Bedingung erfüllen muss")]
        public string Expression
        {
            get => aao.Expression;
            set
            {
                if (aao.Expression == value) return;

                aao.Expression = value;

                RaisePropertyChanged(() => Expression);
            }
        }

        public ObservableCollection<Aao> CombinationList { get; }

        [Display(Description = "AAO die zusätzlich zur aktuellen Bedingung erfüllen sein muss")]
        public Aao SelectedCombination
        {
            get => aao.Combination;
            set
            {
                if (aao.Combination == value) return;

                aao.Combination = value;

                RaisePropertyChanged(() => SelectedCombination);
            }
        }

        public ObservableCollection<Vehicle> VehicleList { get; }

        public ObservableCollection<AaoVehicle> AaoVehicleList
        {
            get { return new ObservableCollection<AaoVehicle>(aao.Vehicles.OrderBy(v => v.Position)); }
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