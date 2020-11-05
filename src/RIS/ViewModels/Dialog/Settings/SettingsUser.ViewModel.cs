#region

using System;
using System.ComponentModel.DataAnnotations;
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
    public class SettingsUserViewModel : ViewModelBase
    {
        private readonly IBusiness business;
        private readonly User user;

        public SettingsUserViewModel(IBusiness _business, int _id)
        {
            try
            {
                business = _business;

                //Query lists for selection  

                //Query item with relations
                user = business.GetUserById(_id);
                if (user == null) user = new User();

                //Do selection from list
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
                business.AddOrUpdateUser(user);

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
            Messenger.Default.Send<SettingsUserDialog>(null);
        }

        #endregion //Commands

        #region Events

        #endregion //Events

        #region Validation

        #endregion //Validation

        #region Public Properties

        public string WindowTitel => "User";

        [Display(Description = "Name des Benutzers")]
        public string Name
        {
            get => user.Name;
            set
            {
                if (user.Name == value) return;

                user.Name = value;

                RaisePropertyChanged(() => Name);
            }
        }

        [Display(Description = "E-Mail Adresse des Benutzers")]
        public string MailAdresse
        {
            get => user.MailAdresse;
            set
            {
                if (user.MailAdresse == value) return;

                user.MailAdresse = value;

                RaisePropertyChanged(() => MailAdresse);
            }
        }

        [Display(Description = "Benachrichtigung des Benutzer per E-Mail bei einem neuen Alarmfax mit Text")]
        public bool FaxMessageService_MailOn
        {
            get => user.FaxMessageService_MailOn;
            set
            {
                if (user.FaxMessageService_MailOn == value) return;

                user.FaxMessageService_MailOn = value;

                RaisePropertyChanged(() => FaxMessageService_MailOn);
            }
        }

        [Display(Description =
            "Benachrichtigung des Benutzer per E-Mail bei einem neuen Alarmfax mit Text und Faxdatei als Anhang")]
        public bool FaxMessageService_FaxOn
        {
            get => user.FaxMessageService_FaxOn;
            set
            {
                if (user.FaxMessageService_FaxOn == value) return;

                user.FaxMessageService_FaxOn = value;

                RaisePropertyChanged(() => FaxMessageService_FaxOn);
            }
        }

        [Display(Description =
            "Benachrichtigung des Benutzer per E-Mail bei einer AMS-Alarmierung mit Audiodatei als Anhang")]
        public bool AlarmMessageService_RecordOn
        {
            get => user.AlarmMessageService_RecordOn;
            set
            {
                if (user.AlarmMessageService_RecordOn == value) return;

                user.AlarmMessageService_RecordOn = value;

                RaisePropertyChanged(() => AlarmMessageService_RecordOn);
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