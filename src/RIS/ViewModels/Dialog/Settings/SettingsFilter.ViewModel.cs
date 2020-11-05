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
    public class SettingsFilterViewModel : ViewModelBase
    {
        private readonly IBusiness business;
        private readonly Filter filter;

        public SettingsFilterViewModel(IBusiness _business, int _id)
        {
            try
            {
                business = _business;

                //Query lists for selection  
                FieldList = new ObservableCollection<FilterField>(business.GetAllFilterFieldAsync().Result);

                //Query item with relations
                filter = business.GetFilterById(_id);
                if (filter == null) filter = new Filter();

                //Do selection from list
                if (filter.Field != null)
                    SelectedField = FieldList.Where(f => f.Id == filter.Field.Id).SingleOrDefault();
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
            return true;
        }

        private void OnSave()
        {
            try
            {
                business.AddOrUpdateFilter(filter);

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
            Messenger.Default.Send<SettingsFilterDialog>(null);
        }

        #endregion //Commands

        #region Events

        #endregion //Events

        #region Validation

        #endregion //Validation

        #region Public Properties

        public string WindowTitel => "Filter";

        [Display(Description = "Zeichenfolge die ersetzt werden soll")]
        public string SearchExpression
        {
            get => filter.SearchExpression;
            set
            {
                if (filter.SearchExpression == value) return;

                filter.SearchExpression = value;

                RaisePropertyChanged(() => SearchExpression);
            }
        }

        [Display(Description =
            "Zeichenfolge die eingefügt wird.\r\nUm eine Zeichenfolge zu Löschen, dieses Feld leer lassen")]
        public string ReplaceExpression
        {
            get => filter.ReplaceExpression;
            set
            {
                if (filter.ReplaceExpression == value) return;

                filter.ReplaceExpression = value;

                RaisePropertyChanged(() => ReplaceExpression);
            }
        }

        [Display(Description = "Filterung wird vor der Anzeige ausgeführt und hat auf die Auswertung keinen Einfluss")]
        public bool DoBeforeShow
        {
            get => filter.DoBeforeShow;
            set
            {
                if (filter.DoBeforeShow == value) return;

                filter.DoBeforeShow = value;

                RaisePropertyChanged(() => DoBeforeShow);
            }
        }

        public ObservableCollection<FilterField> FieldList { get; }

        [Display(Description = "Filter wird auf dieses Feld angewendet")]
        public FilterField SelectedField
        {
            get => filter.Field;
            set
            {
                if (filter.Field == value) return;

                filter.Field = value;

                RaisePropertyChanged(() => SelectedField);
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