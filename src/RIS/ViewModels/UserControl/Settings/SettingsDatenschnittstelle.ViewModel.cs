#region

using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using RIS.Business;
using RIS.Core;
using RIS.Model;
using RIS.Properties;
using RIS.Views;
using SRS.Utilities;
using MessageBox = RIS.Core.Helper.MessageBox;

#endregion

namespace RIS.ViewModels
{
    public class SettingsDatenschnittstelleViewModel : ViewModelBase
    {
        private readonly IAlarmappService _alarmappService;
        private readonly IBusiness _business;

        public SettingsDatenschnittstelleViewModel()
        {
            try
            {
                _business = ServiceLocator.Current.GetInstance<IBusiness>();
                _alarmappService = ServiceLocator.Current.GetInstance<IAlarmappService>();

                LoadAmsList();
                LoadFileprintList();
                LoadPrinterList();
                LoadUserList();
                LoadAlarmappGroupList();

                //Messenger refresh affected list
                Messenger.Default.Register<SettingsAlarmappDialog>(this,
                    notification => DispatcherHelper.CheckBeginInvokeOnUI(() => { LoadAlarmappGroupList(); }));
                Messenger.Default.Register<SettingsAmsDialog>(this,
                    notification => DispatcherHelper.CheckBeginInvokeOnUI(() => { LoadAmsList(); }));
                Messenger.Default.Register<SettingsFileprintDialog>(this,
                    notification => DispatcherHelper.CheckBeginInvokeOnUI(() => { LoadFileprintList(); }));
                Messenger.Default.Register<SettingsPrinterDialog>(this,
                    notification => DispatcherHelper.CheckBeginInvokeOnUI(() => { LoadPrinterList(); }));
                Messenger.Default.Register<SettingsUserDialog>(this,
                    notification => DispatcherHelper.CheckBeginInvokeOnUI(() => { LoadUserList(); }));
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #region Public Functions

        public void Save()
        {
        }

        #endregion //Public Functions

        #region Commands

        #region Fileprint

        private RelayCommand addFileprintCommand;

        [Display(Description = "Öffnet den Dialog zum Hinzufügen einer neuen Fileprint")]
        public RelayCommand AddFileprintCommand
        {
            get
            {
                if (addFileprintCommand == null)
                    addFileprintCommand = new RelayCommand(() => OnAddFileprint(), () => CanAddFileprint());

                return addFileprintCommand;
            }
        }

        private bool CanAddFileprint()
        {
            return true;
        }

        private void OnAddFileprint()
        {
            try
            {
                Logger.WriteDebug("Settings: AddFileprintCommand");

                Messenger.Default.Send(new SettingsFileprintDialog(_business, 0));
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand<object> editFileprintCommand;

        [Display(Description = "Öffnet den Dialog zum Bearbeiten der Fileprint")]
        public RelayCommand<object> EditFileprintCommand
        {
            get
            {
                if (editFileprintCommand == null)
                    editFileprintCommand = new RelayCommand<object>(param => OnEditFileprint(param),
                        param => CanEditFileprint(param));

                return editFileprintCommand;
            }
        }

        private bool CanEditFileprint(object param)
        {
            if (param == null) return false;

            if (FileprintList == null || FileprintList.Count <= 0) return false;

            if (FileprintList.Where(c => c.Id == ((Fileprint) param).Id).SingleOrDefault() == null) return false;

            return true;
        }

        private void OnEditFileprint(object param)
        {
            try
            {
                Logger.WriteDebug("Settings: EditFileprintCommand");

                Messenger.Default.Send(new SettingsFileprintDialog(_business, ((Fileprint) param).Id));
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand<object> deleteFileprintCommand;

        [Display(Description = "Entfernt diese Fileprint")]
        public RelayCommand<object> DeleteFileprintCommand
        {
            get
            {
                if (deleteFileprintCommand == null)
                    deleteFileprintCommand = new RelayCommand<object>(param => OnDeleteFileprint(param),
                        param => CanDeleteFileprint(param));

                return deleteFileprintCommand;
            }
        }

        private bool CanDeleteFileprint(object param)
        {
            if (param == null) return false;

            if (FileprintList == null || FileprintList.Count <= 0) return false;

            if (FileprintList.Where(c => c.Id == ((Fileprint) param).Id).SingleOrDefault() == null) return false;

            return true;
        }

        private void OnDeleteFileprint(object param)
        {
            try
            {
                Logger.WriteDebug("Settings: DeleteFileprintCommand");

                _business.DeleteFileprint((Fileprint) param);
                LoadFileprintList();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion //Fileprint

        #region Printer

        private RelayCommand initializePrintersCommand;

        [Display(Description =
            "Löscht alle Drucker mit den Einstellungen aus der Datenbank und ruft alle am System installierten neu ab")]
        public RelayCommand InitializePrintersCommand
        {
            get
            {
                if (initializePrintersCommand == null)
                    initializePrintersCommand =
                        new RelayCommand(() => OnInitializePrinters(), () => CanInitializePrinters());

                return initializePrintersCommand;
            }
        }

        private bool CanInitializePrinters()
        {
            return true;
        }

        private void OnInitializePrinters()
        {
            try
            {
                Logger.WriteDebug("Settings: InitializePrintersCommand");

                ServiceLocator.Current.GetInstance<WaitSplashScreen>().Show();

                _business.DeletePrinters();
                _business.LoadPrinters();

                LoadPrinterList();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                ServiceLocator.Current.GetInstance<WaitSplashScreen>().Close();
            }
        }

        private RelayCommand<object> editPrinterCommand;

        [Display(Description = "Öffnet den Dialog zum Bearbeiten des Druckers")]
        public RelayCommand<object> EditPrinterCommand
        {
            get
            {
                if (editPrinterCommand == null)
                    editPrinterCommand =
                        new RelayCommand<object>(param => OnEditPrinter(param), param => CanEditPrinter(param));

                return editPrinterCommand;
            }
        }

        private bool CanEditPrinter(object param)
        {
            if (param == null) return false;

            if (PrinterList == null || PrinterList.Count <= 0) return false;

            if (PrinterList.Where(c => c.Id == ((Printer) param).Id).SingleOrDefault() == null) return false;

            return true;
        }

        private void OnEditPrinter(object param)
        {
            try
            {
                Logger.WriteDebug("Settings: EditPrinterCommand");

                Messenger.Default.Send(new SettingsPrinterDialog(_business, ((Printer) param).Id));
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion //Printer

        #region User

        private RelayCommand addUserCommand;

        [Display(Description = "Öffnet den Dialog zum Hinzufügen einer neuen User")]
        public RelayCommand AddUserCommand
        {
            get
            {
                if (addUserCommand == null) addUserCommand = new RelayCommand(() => OnAddUser(), () => CanAddUser());

                return addUserCommand;
            }
        }

        private bool CanAddUser()
        {
            return true;
        }

        private void OnAddUser()
        {
            try
            {
                Logger.WriteDebug("Settings: AddUserCommand");

                Messenger.Default.Send(new SettingsUserDialog(_business, 0));
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand<object> editUserCommand;

        [Display(Description = "Öffnet den Dialog zum Bearbeiten der User")]
        public RelayCommand<object> EditUserCommand
        {
            get
            {
                if (editUserCommand == null)
                    editUserCommand = new RelayCommand<object>(param => OnEditUser(param), param => CanEditUser(param));

                return editUserCommand;
            }
        }

        private bool CanEditUser(object param)
        {
            if (param == null) return false;

            if (UserList == null || UserList.Count <= 0) return false;

            if (UserList.Where(c => c.Id == ((User) param).Id).SingleOrDefault() == null) return false;

            return true;
        }

        private void OnEditUser(object param)
        {
            try
            {
                Logger.WriteDebug("Settings: EditUserCommand");

                Messenger.Default.Send(new SettingsUserDialog(_business, ((User) param).Id));
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand<object> deleteUserCommand;

        [Display(Description = "Entfernt diese User")]
        public RelayCommand<object> DeleteUserCommand
        {
            get
            {
                if (deleteUserCommand == null)
                    deleteUserCommand =
                        new RelayCommand<object>(param => OnDeleteUser(param), param => CanDeleteUser(param));

                return deleteUserCommand;
            }
        }

        private bool CanDeleteUser(object param)
        {
            if (param == null) return false;

            if (UserList == null || UserList.Count <= 0) return false;

            if (UserList.Where(c => c.Id == ((User) param).Id).SingleOrDefault() == null) return false;

            return true;
        }

        private void OnDeleteUser(object param)
        {
            try
            {
                Logger.WriteDebug("Settings: DeleteUserCommand");

                _business.DeleteUser((User) param);
                LoadUserList();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion //User

        #region Ams

        private RelayCommand addAmsCommand;

        [Display(Description = "Öffnet den Dialog zum Hinzufügen einer neuen Ams")]
        public RelayCommand AddAmsCommand
        {
            get
            {
                if (addAmsCommand == null) addAmsCommand = new RelayCommand(() => OnAddAms(), () => CanAddAms());

                return addAmsCommand;
            }
        }

        private bool CanAddAms()
        {
            return true;
        }

        private void OnAddAms()
        {
            try
            {
                Logger.WriteDebug("Settings: AddAmsCommand");

                Messenger.Default.Send(new SettingsAmsDialog(_business, 0));
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand<object> editAmsCommand;

        [Display(Description = "Öffnet den Dialog zum Bearbeiten der Ams")]
        public RelayCommand<object> EditAmsCommand
        {
            get
            {
                if (editAmsCommand == null)
                    editAmsCommand = new RelayCommand<object>(param => OnEditAms(param), param => CanEditAms(param));

                return editAmsCommand;
            }
        }

        private bool CanEditAms(object param)
        {
            if (param == null) return false;

            if (AmsList == null || AmsList.Count <= 0) return false;

            if (AmsList.Where(c => c.Id == ((Ams) param).Id).SingleOrDefault() == null) return false;

            return true;
        }

        private void OnEditAms(object param)
        {
            try
            {
                Logger.WriteDebug("Settings: EditAmsCommand");

                Messenger.Default.Send(new SettingsAmsDialog(_business, ((Ams) param).Id));
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand<object> deleteAmsCommand;

        [Display(Description = "Entfernt diese Ams")]
        public RelayCommand<object> DeleteAmsCommand
        {
            get
            {
                if (deleteAmsCommand == null)
                    deleteAmsCommand =
                        new RelayCommand<object>(param => OnDeleteAms(param), param => CanDeleteAms(param));

                return deleteAmsCommand;
            }
        }

        private bool CanDeleteAms(object param)
        {
            if (param == null) return false;

            if (AmsList == null || AmsList.Count <= 0) return false;

            if (AmsList.Where(c => c.Id == ((Ams) param).Id).SingleOrDefault() == null) return false;

            return true;
        }

        private void OnDeleteAms(object param)
        {
            try
            {
                Logger.WriteDebug("Settings: DeleteAmsCommand");

                _business.DeleteAms((Ams) param);
                LoadAmsList();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion //Ams

        #region Alarmapp

        private RelayCommand alarmappClearCommand;

        [Display(Description = "")]
        public RelayCommand AlarmappClearCommand
        {
            get
            {
                if (alarmappClearCommand == null)
                    alarmappClearCommand =
                        new RelayCommand(() => OnAlarmappClearCommand(), () => CanAlarmappClearCommande());

                return alarmappClearCommand;
            }
        }

        private bool CanAlarmappClearCommande()
        {
            return true;
        }

        private void OnAlarmappClearCommand()
        {
            try
            {
                Logger.WriteDebug("Settings: AlarmappClearCommand");

                var _clearResult = _alarmappService.ClearAlarmgroups();
                if (_clearResult) LoadAlarmappGroupList();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand alarmappRefreshCommand;

        [Display(Description = "")]
        public RelayCommand AlarmappRefreshCommand
        {
            get
            {
                if (alarmappRefreshCommand == null)
                    alarmappRefreshCommand = new RelayCommand(() => OnAlarmappRefreshCommand(),
                        () => CanAlarmappRefreshCommande());

                return alarmappRefreshCommand;
            }
        }

        private bool CanAlarmappRefreshCommande()
        {
            return true;
        }

        private void OnAlarmappRefreshCommand()
        {
            try
            {
                Logger.WriteDebug("Settings: AlarmappRefreshCommand");
                ServiceLocator.Current.GetInstance<WaitSplashScreen>().Show();

                var _refreshResult = _alarmappService.Refresh(AlarmappApiToken, AlarmappOrganisationId);
                if (_refreshResult) LoadAlarmappGroupList();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                ServiceLocator.Current.GetInstance<WaitSplashScreen>().Close();
            }
        }

        private RelayCommand<object> editAlarmappGroupCommand;

        [Display(Description = "Öffnet den Dialog zum Bearbeiten der AlarmApp Gruppe")]
        public RelayCommand<object> EditAlarmappGroupCommand
        {
            get
            {
                if (editAlarmappGroupCommand == null)
                    editAlarmappGroupCommand = new RelayCommand<object>(param => OnEditAlarmappGroup(param),
                        param => CanEditAlarmappGroup(param));

                return editAlarmappGroupCommand;
            }
        }

        private bool CanEditAlarmappGroup(object param)
        {
            if (param == null) return false;

            if (AlarmappGroupList == null || AlarmappGroupList.Count <= 0) return false;

            if (AlarmappGroupList.Where(c => c.Id == ((AlarmappGroup) param).Id).SingleOrDefault() == null)
                return false;

            return true;
        }

        private void OnEditAlarmappGroup(object param)
        {
            try
            {
                Logger.WriteDebug("Settings: EditAlarmappGroupCommand");

                Messenger.Default.Send(new SettingsAlarmappDialog(_business, ((AlarmappGroup) param).Id));
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion //Alarmapp

        #endregion //Commands

        #region Events

        #endregion //Events

        #region Validation

        #endregion //Validation

        #region Public Properties

        #region Decoder

        public Array Decoder_ModeList => Enum.GetValues(typeof(DecoderMode));

        [Display(Description =
            "IP-Adresse oder Hostname des Auswertungsserver\n\nBeispiel lokal: 127.0.0.1\nTETRACONTROL: wird nicht verwendet")]
        public string Decoder1_Server
        {
            get => Settings.Default.Decoder1_Server;
            set
            {
                if (Settings.Default.Decoder1_Server == value) return;

                Settings.Default.Decoder1_Server = value;

                RaisePropertyChanged(() => Decoder1_Server);
            }
        }

        [Display(Description = "Port des Auswertungsserver\n\nStandard FMS32: 9300")]
        public int Decoder1_Port
        {
            get => Settings.Default.Decoder1_Port;
            set
            {
                if (Settings.Default.Decoder1_Port == value) return;

                Settings.Default.Decoder1_Port = value;

                RaisePropertyChanged(() => Decoder1_Port);
            }
        }

        [Display(Description =
            "Auswahl des Auswertungsserver\n\nTETRACONTROL: Einstellungen->Optionen->Aktionen->An Webserver senden: http://localhost:8081")]
        public DecoderMode Decoder1_Mode
        {
            get
            {
                Enum.GetValues(typeof(DecoderMode));
                return Settings.Default.Decoder1_Mode;
            }
            set
            {
                if (Settings.Default.Decoder1_Mode == value) return;

                Settings.Default.Decoder1_Mode = value;

                RaisePropertyChanged(() => Decoder1_Mode);
            }
        }

        [Display(Description =
            "IP-Adresse oder Hostname des Auswertungsserver\n\nBeispiel lokal: 127.0.0.1\nTETRACONTROL: wird nicht verwendet")]
        public string Decoder2_Server
        {
            get => Settings.Default.Decoder2_Server;
            set
            {
                if (Settings.Default.Decoder2_Server == value) return;

                Settings.Default.Decoder2_Server = value;

                RaisePropertyChanged(() => Decoder2_Server);
            }
        }

        [Display(Description = "Port des Auswertungsserver\n\nStandard FMS32: 9300")]
        public int Decoder2_Port
        {
            get => Settings.Default.Decoder2_Port;
            set
            {
                if (Settings.Default.Decoder2_Port == value) return;

                Settings.Default.Decoder2_Port = value;

                RaisePropertyChanged(() => Decoder2_Port);
            }
        }

        [Display(Description =
            "IP-Adresse oder Hostname des Auswertungsserver\n\nBeispiel lokal: 127.0.0.1\nTETRACONTROL: wird nicht verwendet")]
        public DecoderMode Decoder2_Mode
        {
            get
            {
                Enum.GetValues(typeof(DecoderMode));
                return Settings.Default.Decoder2_Mode;
            }
            set
            {
                if (Settings.Default.Decoder2_Mode == value) return;

                Settings.Default.Decoder2_Mode = value;

                RaisePropertyChanged(() => Decoder2_Mode);
            }
        }

        #endregion //Decoder

        #region Dateien

        private ObservableCollection<Fileprint> fileprintList;

        public ObservableCollection<Fileprint> FileprintList
        {
            get => fileprintList;
            set
            {
                if (fileprintList == value) return;

                fileprintList = value;

                RaisePropertyChanged(() => FileprintList);
            }
        }

        #endregion //Dateien

        #region Printer

        private ObservableCollection<Printer> printerList;

        public ObservableCollection<Printer> PrinterList
        {
            get => printerList;
            set
            {
                if (printerList == value) return;

                printerList = value;

                RaisePropertyChanged(() => PrinterList);
            }
        }

        #endregion //Printer

        #region User

        private ObservableCollection<User> userList;

        public ObservableCollection<User> UserList
        {
            get => userList;
            set
            {
                if (userList == value) return;

                userList = value;

                RaisePropertyChanged(() => UserList);
            }
        }

        #endregion //User

        #region Mail
        
        [Display(Description = "IP oder Hostname des SMTP-Servers zum versenden von E-Mails")]
        public string MailOutput_Server
        {
            get => Settings.Default.MailOutput_Server;
            set
            {
                if (Settings.Default.MailOutput_Server == value) return;

                Settings.Default.MailOutput_Server = value;

                RaisePropertyChanged(() => MailOutput_Server);
            }
        }

        [Display(Description =
            "Aktivieren falls der SMTP-Servers eine SSL-Verschlüsselung benötigt. Für TLS nicht aktivieren!")]
        public bool MailOutput_SSL
        {
            get => Settings.Default.MailOutput_SSL;
            set
            {
                if (Settings.Default.MailOutput_SSL == value) return;

                Settings.Default.MailOutput_SSL = value;

                RaisePropertyChanged(() => MailOutput_SSL);
            }
        }

        [Display(Description = "Port des SMTP-Servers zum versenden von E-Mails")]
        public int MailOutput_Port
        {
            get => Settings.Default.MailOutput_Port;
            set
            {
                if (Settings.Default.MailOutput_Port == value) return;

                Settings.Default.MailOutput_Port = value;

                RaisePropertyChanged(() => MailOutput_Port);
            }
        }

        [Display(Description = "Absender der E-Mails")]
        public string MailOutput_Sender
        {
            get => Settings.Default.MailOutput_Sender;
            set
            {
                if (Settings.Default.MailOutput_Sender == value) return;

                Settings.Default.MailOutput_Sender = value;

                RaisePropertyChanged(() => MailOutput_Sender);
            }
        }

        [Display(Description = "Benutzer um sich am SMTP-Servers anzumelden")]
        public string MailOutput_User
        {
            get => Settings.Default.MailOutput_User;
            set
            {
                if (Settings.Default.MailOutput_User == value) return;

                Settings.Default.MailOutput_User = value;

                RaisePropertyChanged(() => MailOutput_User);
            }
        }

        [Display(Description = "Passwort des Benutzers um sich am SMTP-Servers anzumelden")]
        public string MailOutput_Password
        {
            get => null;
            set { RaisePropertyChanged(() => MailOutput_Password); }
        }

        [Display(Description = "Betreff der E-Mail die bei einem Alarmfax versendet wird")]
        public string MailOutput_Subject
        {
            get => Settings.Default.MailOutput_Subject;
            set
            {
                if (Settings.Default.MailOutput_Subject == value) return;

                Settings.Default.MailOutput_Subject = value;

                RaisePropertyChanged(() => MailOutput_Subject);
            }
        }

        #endregion //Mail

        #region Ams

        [Display(Description = "Betreff der Alarm-Message-Service E-Mail versendet wird")]
        public string MailAms_Subject
        {
            get => Settings.Default.MailAms_Subject;
            set
            {
                if (Settings.Default.MailAms_Subject == value) return;

                Settings.Default.MailAms_Subject = value;

                RaisePropertyChanged(() => MailAms_Subject);
            }
        }

        private ObservableCollection<Ams> amsList;

        public ObservableCollection<Ams> AmsList
        {
            get => amsList;
            set
            {
                if (amsList == value) return;

                amsList = value;

                RaisePropertyChanged(() => AmsList);
            }
        }

        #endregion //Ams

        #region Alarmapp
        
        [Display(Description = "")]
        public string AlarmappApiToken
        {
            get => Settings.Default.Alarmapp_ApiToken;
            set
            {
                if (Settings.Default.Alarmapp_ApiToken == value) return;

                Settings.Default.Alarmapp_ApiToken = value;

                RaisePropertyChanged(() => AlarmappApiToken);
            }
        }

        [Display(Description = "")]
        public string AlarmappOrganisationId
        {
            get => Settings.Default.Alarmapp_OrganisationId;
            set
            {
                if (Settings.Default.Alarmapp_OrganisationId == value) return;

                Settings.Default.Alarmapp_OrganisationId = value;

                RaisePropertyChanged(() => AlarmappOrganisationId);
            }
        }

        private ObservableCollection<AlarmappGroup> alarmappgroupList;

        public ObservableCollection<AlarmappGroup> AlarmappGroupList
        {
            get => alarmappgroupList;
            set
            {
                if (alarmappgroupList == value) return;

                alarmappgroupList = value;

                RaisePropertyChanged(() => AlarmappGroupList);
            }
        }

        #endregion //Alarmapp

        #region Fireboard

        [Display(Description = "")]
        public string FireboardAuthKey
        {
            get => Settings.Default.Fireboard_AuthKey;
            set
            {
                if (Settings.Default.Fireboard_AuthKey == value) return;

                Settings.Default.Fireboard_AuthKey = value;

                RaisePropertyChanged(() => FireboardAuthKey);
            }
        }

        #endregion //Alarmapp

        #endregion //Public Properties

        #region Private Properties

        #endregion //Private Properties

        #region Private Functions

        private void LoadFileprintList()
        {
            fileprintList = new ObservableCollection<Fileprint>(_business.GetAllFileprintAsync().Result);
            RaisePropertyChanged(() => FileprintList);
        }

        private void LoadPrinterList()
        {
            printerList = new ObservableCollection<Printer>(_business.GetPrintersOverviewAsync().Result);
            RaisePropertyChanged(() => PrinterList);
        }

        private void LoadUserList()
        {
            userList = new ObservableCollection<User>(_business.GetAllUserAsync().Result);
            RaisePropertyChanged(() => UserList);
        }

        private void LoadAmsList()
        {
            amsList = new ObservableCollection<Ams>(_business.GetAmsOverviewAsync().Result);
            RaisePropertyChanged(() => AmsList);
        }

        private void LoadAlarmappGroupList()
        {
            if (string.IsNullOrEmpty(Settings.Default.Alarmapp_ApiToken)) return;

            alarmappgroupList =
                new ObservableCollection<AlarmappGroup>(_business.GetAlarmappGroupOverviewAsync().Result);
            RaisePropertyChanged(() => AlarmappGroupList);
        }

        #endregion //Private Funtions
    }
}