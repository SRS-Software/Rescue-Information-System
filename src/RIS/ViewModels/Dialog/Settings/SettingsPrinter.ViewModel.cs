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
    public class SettingsPrinterViewModel : ViewModelBase
    {
        private readonly IBusiness business;
        private readonly Printer printer;

        public SettingsPrinterViewModel(IBusiness _business, int _id)
        {
            try
            {
                business = _business;

                //Query lists for selection  
                FileprintList = new ObservableCollection<Fileprint>(business.GetAllFileprintAsync().Result);

                //Query item with relations
                printer = business.GetPrinterById(_id);
                if (printer == null) printer = new Printer();

                //Do selection from list
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #region Commands

        private RelayCommand<object> addFileprintCommand;

        public RelayCommand<object> AddFileprintCommand
        {
            get
            {
                if (addFileprintCommand == null)
                    addFileprintCommand =
                        new RelayCommand<object>(param => OnAddFileprint(param), param => CanAddFileprint(param));

                return addFileprintCommand;
            }
        }

        private bool CanAddFileprint(object param)
        {
            if (param == null) return false;

            if (printer.Fileprints == null) return false;

            if (printer.Fileprints.Where(a => a.Id == ((Fileprint) param).Id).SingleOrDefault() != null) return false;

            return true;
        }

        private void OnAddFileprint(object param)
        {
            printer.Fileprints.Add((Fileprint) param);
            RaisePropertyChanged(() => PrinterFileprintList);
        }

        private RelayCommand<object> removeFileprintCommand;

        public RelayCommand<object> RemoveFileprintCommand
        {
            get
            {
                if (removeFileprintCommand == null)
                    removeFileprintCommand = new RelayCommand<object>(param => OnRemoveFileprint(param),
                        param => CanRemoveFileprint(param));

                return removeFileprintCommand;
            }
        }

        private bool CanRemoveFileprint(object param)
        {
            if (param == null) return false;

            if (printer.Fileprints == null) return false;

            if (printer.Fileprints.Where(a => a.Id == ((Fileprint) param).Id).SingleOrDefault() == null) return false;

            return true;
        }

        private void OnRemoveFileprint(object param)
        {
            printer.Fileprints.Remove((Fileprint) param);
            RaisePropertyChanged(() => PrinterFileprintList);
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
            if (printer == null) return false;

            return true;
        }

        private void OnSave()
        {
            try
            {
                business.AddOrUpdatePrinterAsync(printer);

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
            Messenger.Default.Send<SettingsPrinterDialog>(null);
        }

        #endregion //Commands

        #region Events

        #endregion //Events

        #region Validation

        #endregion //Validation

        #region Public Properties

        public string WindowTitel => printer.PrinterName;


        [Display(Description = "Anzahl der Kopien die mindestens ausgedruckt werden")]
        public int FaxCopies
        {
            get => printer.FaxCopies;
            set
            {
                if (printer.FaxCopies == value) return;

                printer.FaxCopies = value;

                RaisePropertyChanged(() => FaxCopies);
            }
        }

        [Display(Description = "Dynamische Anzahl von Kopien die zusätzlich mit ausgedruckt werden")]
        public bool FaxNumberOfVehiclesOn
        {
            get => printer.FaxNumberOfVehiclesOn;
            set
            {
                if (printer.FaxNumberOfVehiclesOn == value) return;

                printer.FaxNumberOfVehiclesOn = value;

                RaisePropertyChanged(() => FaxNumberOfVehiclesOn);
            }
        }

        [Display(Description = "Anzahl der Kopien die mindestens ausgedruckt werden")]
        public int ReportCopies
        {
            get => printer.ReportCopies;
            set
            {
                if (printer.ReportCopies == value) return;

                printer.ReportCopies = value;

                RaisePropertyChanged(() => ReportCopies);
            }
        }

        [Display(Description = "Dynamische Anzahl von Kopien die zusätzlich mit ausgedruckt werden")]
        public bool ReportNumberOfVehiclesOn
        {
            get => printer.ReportNumberOfVehiclesOn;
            set
            {
                if (printer.ReportNumberOfVehiclesOn == value) return;

                printer.ReportNumberOfVehiclesOn = value;

                RaisePropertyChanged(() => ReportNumberOfVehiclesOn);
            }
        }

        [Display(Description = "Entfernung zum Einsatzort muss gößer sein um einen Einsatzbericht zu drucken")]
        public int ReportDistance
        {
            get => printer.ReportDistance;
            set
            {
                if (printer.ReportDistance == value) return;

                printer.ReportDistance = value;

                RaisePropertyChanged(() => ReportDistance);
            }
        }

        [Display(Description = "Schlagwort auf dem Einsatzbericht")]
        public bool ReportDataSchlagwortOn
        {
            get => printer.ReportDataSchlagwortOn;
            set
            {
                if (printer.ReportDataSchlagwortOn == value) return;

                printer.ReportDataSchlagwortOn = value;

                RaisePropertyChanged(() => ReportDataSchlagwortOn);
            }
        }

        [Display(Description = "Stichwort auf dem Einsatzbericht")]
        public bool ReportDataStichwortOn
        {
            get => printer.ReportDataStichwortOn;
            set
            {
                if (printer.ReportDataStichwortOn == value) return;

                printer.ReportDataStichwortOn = value;

                RaisePropertyChanged(() => ReportDataStichwortOn);
            }
        }

        [Display(Description = "Ort auf dem Einsatzbericht")]
        public bool ReportDataOrtOn
        {
            get => printer.ReportDataOrtOn;
            set
            {
                if (printer.ReportDataOrtOn == value) return;

                printer.ReportDataOrtOn = value;

                RaisePropertyChanged(() => ReportDataOrtOn);
            }
        }

        [Display(Description = "Straße auf dem Einsatzbericht")]
        public bool ReportDataStraßeOn
        {
            get => printer.ReportDataStraßeOn;
            set
            {
                if (printer.ReportDataStraßeOn == value) return;

                printer.ReportDataStraßeOn = value;

                RaisePropertyChanged(() => ReportDataStraßeOn);
            }
        }

        [Display(Description = "Objekt auf dem Einsatzbericht")]
        public bool ReportDataObjektOn
        {
            get => printer.ReportDataObjektOn;
            set
            {
                if (printer.ReportDataObjektOn == value) return;

                printer.ReportDataObjektOn = value;

                RaisePropertyChanged(() => ReportDataObjektOn);
            }
        }

        [Display(Description = "Station auf dem Einsatzbericht")]
        public bool ReportDataStationOn
        {
            get => printer.ReportDataStationOn;
            set
            {
                if (printer.ReportDataStationOn == value) return;

                printer.ReportDataStationOn = value;

                RaisePropertyChanged(() => ReportDataStationOn);
            }
        }

        [Display(Description = "Kreuzung auf dem Einsatzbericht")]
        public bool ReportDataKreuzungOn
        {
            get => printer.ReportDataKreuzungOn;
            set
            {
                if (printer.ReportDataKreuzungOn == value) return;

                printer.ReportDataKreuzungOn = value;

                RaisePropertyChanged(() => ReportDataKreuzungOn);
            }
        }

        [Display(Description = "Abschnitt auf dem Einsatzbericht")]
        public bool ReportDataAbschnittOn
        {
            get => printer.ReportDataAbschnittOn;
            set
            {
                if (printer.ReportDataAbschnittOn == value) return;

                printer.ReportDataAbschnittOn = value;

                RaisePropertyChanged(() => ReportDataAbschnittOn);
            }
        }

        [Display(Description = "Bemerkung auf dem Einsatzbericht")]
        public bool ReportDataBemerkungOn
        {
            get => printer.ReportDataBemerkungOn;
            set
            {
                if (printer.ReportDataBemerkungOn == value) return;

                printer.ReportDataBemerkungOn = value;

                RaisePropertyChanged(() => ReportDataBemerkungOn);
            }
        }

        [Display(Description = "Eigene alarmierte Fahrzeuge mit auf dem Einsatzbericht")]
        public bool ReportVehiclesOn
        {
            get => printer.ReportVehiclesOn;
            set
            {
                if (printer.ReportVehiclesOn == value) return;

                printer.ReportVehiclesOn = value;

                RaisePropertyChanged(() => ReportVehiclesOn);
            }
        }

        [Display(Description = "Bild der Anfahrt auf dem Einsatzbericht")]
        public bool ReportRouteImageOn
        {
            get => printer.ReportRouteImageOn;
            set
            {
                if (printer.ReportRouteImageOn == value) return;

                printer.ReportRouteImageOn = value;

                RaisePropertyChanged(() => ReportRouteImageOn);
            }
        }

        [Display(Description = "Anfahrtsbeschreibung auf dem Einsatzbericht")]
        public bool ReportRouteDescriptionOn
        {
            get => printer.ReportRouteDescriptionOn;
            set
            {
                if (printer.ReportRouteDescriptionOn == value) return;

                printer.ReportRouteDescriptionOn = value;

                RaisePropertyChanged(() => ReportRouteDescriptionOn);
            }
        }

        [Display(Description = "Anzahl der Kopien die mindestens ausgedruckt werden")]
        public int FileprintCopies
        {
            get => printer.FileprintCopies;
            set
            {
                if (printer.FileprintCopies == value) return;

                printer.FileprintCopies = value;

                RaisePropertyChanged(() => FileprintCopies);
            }
        }

        [Display(Description = "Dynamische Anzahl von Kopien die zusätzlich mit ausgedruckt werden")]
        public bool FileprintNumberOfVehiclesOn
        {
            get => printer.FileprintNumberOfVehiclesOn;
            set
            {
                if (printer.FileprintNumberOfVehiclesOn == value) return;

                printer.FileprintNumberOfVehiclesOn = value;

                RaisePropertyChanged(() => FileprintNumberOfVehiclesOn);
            }
        }

        public ObservableCollection<Fileprint> FileprintList { get; }

        public ObservableCollection<Fileprint> PrinterFileprintList
        {
            get { return new ObservableCollection<Fileprint>(printer.Fileprints.OrderBy(f => f.Name)); }
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