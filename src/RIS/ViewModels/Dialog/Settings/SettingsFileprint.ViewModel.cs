#region

using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
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
    public class SettingsFileprintViewModel : ViewModelBase
    {
        private readonly IBusiness business;
        private readonly Fileprint fileprint;

        public SettingsFileprintViewModel(IBusiness _business, int _id)
        {
            try
            {
                business = _business;

                //Query lists for selection   
                ConditionList =
                    new ObservableCollection<FileprintCondition>(business.GetAllFileprintConditionAsync().Result);

                //Query item with relations
                fileprint = business.GetFileprintById(_id);
                if (fileprint == null) fileprint = new Fileprint();

                //Do selection from list
                if (fileprint.Condition != null)
                    SelectedCondition = ConditionList.Where(c => c.Id == fileprint.Condition.Id).SingleOrDefault();
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
                    Title = "Wählen Sie die gewünschte Datei aus:",
                    RestoreDirectory = true,
                    InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath),
                    Filter = "PDF (*.PDF) | *.PDF",
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
                business.AddOrUpdateFileprint(fileprint);

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
            Messenger.Default.Send<SettingsFileprintDialog>(null);
        }

        #endregion //Commands

        #region Events

        #endregion //Events

        #region Validation

        #endregion //Validation

        #region Public Properties

        public string WindowTitel => "Dateiausdruck";

        [Display(Description = "Beschreibung")]
        public string Name
        {
            get => fileprint.Name;
            set
            {
                if (fileprint.Name == value) return;

                fileprint.Name = value;

                RaisePropertyChanged(() => Name);
            }
        }

        public ObservableCollection<FileprintCondition> ConditionList { get; }

        [Display(Description = "Bedingung des Dateiausdrucks")]
        public FileprintCondition SelectedCondition
        {
            get => fileprint.Condition;
            set
            {
                if (fileprint.Condition == value) return;

                fileprint.Condition = value;

                RaisePropertyChanged(() => SelectedCondition);
            }
        }

        [Display(Description = "Wert den die Bedingung erfüllen muss")]
        public string Expression
        {
            get => fileprint.Expression;
            set
            {
                if (fileprint.Expression == value) return;

                fileprint.Expression = value;

                RaisePropertyChanged(() => Expression);
            }
        }

        [Display(Description = "PDF-Datei die ausgedruckt werden soll\r\nAdobe-Reader muss installiert sein")]
        public string File
        {
            get => fileprint.File;
            set
            {
                if (fileprint.File == value) return;

                fileprint.File = value;

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