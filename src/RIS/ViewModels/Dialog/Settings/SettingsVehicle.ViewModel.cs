#region

using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using RIS.Business;
using RIS.Model;
using RIS.Views;
using SRS.Utilities;
using Application = System.Windows.Forms.Application;
using MessageBox = RIS.Core.Helper.MessageBox;

#endregion

namespace RIS.ViewModels
{
    public class SettingsVehicleViewModel : ViewModelBase
    {
        private readonly IBusiness business;
        private readonly Vehicle vehicle;

        public SettingsVehicleViewModel(IBusiness _business, int _id)
        {
            try
            {
                business = _business;

                //Query lists for selection  

                //Query item with relations
                vehicle = business.GetVehicleById(_id);
                if (vehicle == null) vehicle = new Vehicle();

                //Do selection from list
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #region Commands

        private RelayCommand selectFileCommand;

        public RelayCommand SelectFileCommand
        {
            get
            {
                if (selectFileCommand == null)
                    selectFileCommand = new RelayCommand(() => OnSelectFile(), () => CanSelectFile());

                return selectFileCommand;
            }
        }

        private bool CanSelectFile()
        {
            return true;
        }

        private void OnSelectFile()
        {
            try
            {
                var _openFileDialog = new OpenFileDialog
                {
                    Title = "Wählen Sie die gewünschte Sounddatei oder Programm aus:",
                    RestoreDirectory = true,
                    InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath),
                    Filter =
                        "Audio (*.WAV; *.MID; *.MIDI; *.WMA; *.MP3; *.OGG)|*.WAV; *.MID; *.MIDI; *.WMA; *.MP3; *.OGG | Application (*.EXE; *.BAT;)|*.EXE; *.BAT;",
                    CheckFileExists = true
                };

                if (_openFileDialog.ShowDialog() == DialogResult.OK) File = _openFileDialog.FileName;
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
            return true;
        }

        private void OnSave()
        {
            try
            {
                business.AddOrUpdateVehicle(vehicle);

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
            Messenger.Default.Send<SettingsVehicleDialog>(null);
        }

        #endregion //Commands

        #region Events

        #endregion //Events

        #region Validation

        #endregion //Validation

        #region Public Properties

        public string WindowTitel => "Fahrzeug";

        [Display(Description = "Beschreibung des Fahrzeuges für den Report und E-Mail")]
        public string Name
        {
            get => vehicle.Name;
            set
            {
                if (vehicle.Name == value) return;

                vehicle.Name = value;

                RaisePropertyChanged(() => Name);
            }
        }

        [Display(Description =
            "Anzeigetext in der Haupt- und Alarmansicht\r\nWird dieser Text nicht eingetragen erscheint das Fahrzeug in keiner Ansicht")]
        public string ViewText
        {
            get => vehicle.ViewText;
            set
            {
                if (vehicle.ViewText == value) return;

                vehicle.ViewText = value;

                RaisePropertyChanged(() => ViewText);
            }
        }

        [Display(Description =
            "Text des Einsatzmittels auf dem Alarmfax.\r\nDieser Text muss eindeutig sein, um das Fahrzeug zu erkennen")]
        public string FaxText
        {
            get => vehicle.FaxText;
            set
            {
                if (vehicle.FaxText == value) return;

                vehicle.FaxText = value;

                RaisePropertyChanged(() => FaxText);
            }
        }

        [Display(Description =
            "Kennung des Fahrzeuges abhängig vom verwendeten Decoder\r\nAnalog: FMS z.B.:6D884211\r\nTetra: ISSI z.B.:12345678\r\n")]
        public string BosIdentifier
        {
            get => vehicle.BosIdentifier;
            set
            {
                if (vehicle.BosIdentifier == value) return;

                vehicle.BosIdentifier = value;

                RaisePropertyChanged(() => BosIdentifier);
            }
        }

        [Display(Description =
            "Auswahl einer Datei die sofort bei der Alarmierung mit Status-C gestartet wird\r\nBei der auswahl einer Audiodateien(.WAV, .MID, .MIDI, .WMA, .MP3, .OGG) wird diese im Hintergrund abgespielt")]
        public string File
        {
            get => vehicle.File;
            set
            {
                if (vehicle.File == value) return;

                vehicle.File = value;

                RaisePropertyChanged(() => File);
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