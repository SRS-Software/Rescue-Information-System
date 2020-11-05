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
    public class SettingsPagerViewModel : ViewModelBase
    {
        private readonly IBusiness business;
        private readonly Pager Pager;

        public SettingsPagerViewModel(IBusiness _business, int _id)
        {
            try
            {
                business = _business;

                //Query lists for selection  

                //Query item with relations
                Pager = business.GetPagerById(_id);
                if (Pager == null) Pager = new Pager();

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
                business.AddOrUpdatePager(Pager);

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
            Messenger.Default.Send<SettingsPagerDialog>(null);
        }

        #endregion //Commands

        #region Events

        #endregion //Events

        #region Validation

        #endregion //Validation

        #region Public Properties

        public string WindowTitel => "Pager";

        [Display(Description = "Beschreibung des Pagers")]
        public string Name
        {
            get => Pager.Name;
            set
            {
                if (Pager.Name == value) return;

                Pager.Name = value;

                RaisePropertyChanged(() => Name);
            }
        }

        [Display(Description =
            "Kennung des Pagers abhängig vom verwendeten Decoders\r\nZVEI: 5-Ton-Folge z.B.:00818\r\nPOCSAG: RIC und SUB-RICz.B.:12345682\r\n")]
        public string Identifier
        {
            get => Pager.Identifier;
            set
            {
                if (Pager.Identifier == value) return;

                Pager.Identifier = value;

                RaisePropertyChanged(() => Identifier);
            }
        }

        [Display(Description =
            "Auswahl einer Datei die sofort bei der Alarmierung gestartet wird\r\nBei der auswahl einer Audiodateien(.WAV, .MID, .MIDI, .WMA, .MP3, .OGG) wird diese im Hintergrund abgespielt")]
        public string File
        {
            get => Pager.File;
            set
            {
                if (Pager.File == value) return;

                Pager.File = value;

                RaisePropertyChanged(() => File);
            }
        }

        [Display(Description = "Ansicht in der Pager-Übersicht mit rotem Hintergrund")]
        public bool Priority
        {
            get => Pager.Priority;
            set
            {
                if (Pager.Priority == value) return;

                Pager.Priority = value;

                RaisePropertyChanged(() => Priority);
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