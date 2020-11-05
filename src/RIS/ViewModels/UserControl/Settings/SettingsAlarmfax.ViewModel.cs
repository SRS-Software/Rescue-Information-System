#region

using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using RIS.Business;
using RIS.Core;
using RIS.Core.Helper;
using RIS.Model;
using RIS.Properties;
using RIS.Views;
using SRS.Utilities;
using MessageBox = RIS.Core.Helper.MessageBox;

#endregion

namespace RIS.ViewModels
{
    public class SettingsAlarmfaxViewModel : ViewModelBase
    {
        private readonly IBusiness business;
        private readonly IFaxService faxService;

        #region Private Properties

        private SettingsOcr settingsOcr;

        #endregion //Private Properties

        public SettingsAlarmfaxViewModel()
        {
            try
            {
                business = ServiceLocator.Current.GetInstance<IBusiness>();
                faxService = ServiceLocator.Current.GetInstance<IFaxService>();

                //Ocr Settings 
                settingsOcr = Serializer.Deserialize<SettingsOcr>(Settings.Default.Fax_OcrSettings);

                LoadAaoList();
                LoadFilterList();

                //Messenger refresh affected list
                Messenger.Default.Register<SettingsAaoDialog>(this,
                    notification => DispatcherHelper.CheckBeginInvokeOnUI(() => { LoadAaoList(); }));
                Messenger.Default.Register<SettingsFilterDialog>(this,
                    notification => DispatcherHelper.CheckBeginInvokeOnUI(() => { LoadFilterList(); }));
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #region Public Functions

        public void Save()
        {
            Settings.Default.Fax_OcrSettings = Serializer.Serialize(settingsOcr);
        }

        #endregion //Public Functions

        #region Commands

        #region Fax

        private RelayCommand fax_PathInputBrowseCommand;

        public RelayCommand Fax_PathInputBrowseCommand
        {
            get
            {
                if (fax_PathInputBrowseCommand == null)
                    fax_PathInputBrowseCommand =
                        new RelayCommand(() => OnFax_PathInputBrowse(), () => CanFax_PathInputBrowse());

                return fax_PathInputBrowseCommand;
            }
        }

        private bool CanFax_PathInputBrowse()
        {
            return true;
        }

        private void OnFax_PathInputBrowse()
        {
            try
            {
                Logger.WriteDebug("Settings: Fax_PathInputBrowseCommand");

                var _folderBrowserDialog = new FolderBrowserDialog();
                _folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
                _folderBrowserDialog.ShowNewFolderButton = true;
                _folderBrowserDialog.Description = "Wählen Sie einen Ordner für den Faxeingang aus:";

                if (_folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    Fax_PathInput = _folderBrowserDialog.SelectedPath;
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand fax_PathArchiveBrowseCommand;

        public RelayCommand Fax_PathArchiveBrowseCommand
        {
            get
            {
                if (fax_PathArchiveBrowseCommand == null)
                    fax_PathArchiveBrowseCommand = new RelayCommand(() => OnFax_PathArchiveBrowse(),
                        () => CanFax_PathArchiveBrowse());

                return fax_PathArchiveBrowseCommand;
            }
        }

        private bool CanFax_PathArchiveBrowse()
        {
            return true;
        }

        private void OnFax_PathArchiveBrowse()
        {
            try
            {
                Logger.WriteDebug("Settings: Fax_PathArchiveBrowseCommand");

                var _folderBrowserDialog = new FolderBrowserDialog();
                _folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
                _folderBrowserDialog.ShowNewFolderButton = true;
                _folderBrowserDialog.Description = "Wählen Sie einen Ordner für das Faxarchiv aus:";

                if (_folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    Fax_PathArchive = _folderBrowserDialog.SelectedPath;
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand testFax_LoadCommand;

        [Display(Description =
            "Öffnet ein Fax um die Einrichtung zu erleichtern.\nMit dem Pfeil unten rechts kann das Bild gezoomt und verschoben werden.")]
        public RelayCommand TestFax_LoadCommand
        {
            get
            {
                if (testFax_LoadCommand == null)
                    testFax_LoadCommand = new RelayCommand(() => OnTestFax_Load(), () => CanTestFax_Load());

                return testFax_LoadCommand;
            }
        }

        private bool CanTestFax_Load()
        {
            return true;
        }

        private void OnTestFax_Load()
        {
            try
            {
                Logger.WriteDebug("Settings: TestFax_LoadCommand");

                var _openFileDialog = new OpenFileDialog();
                _openFileDialog.Filter = "Faxfile (*.tiff;*.tif)|*.tiff;*.tif";
                if (_openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    fax_ImagePath = _openFileDialog.FileName;

                    // Open a Uri and decode a BMP image
                    var _tiffDecoder = new TiffBitmapDecoder(
                        new Uri(fax_ImagePath, UriKind.RelativeOrAbsolute), BitmapCreateOptions.PreservePixelFormat,
                        BitmapCacheOption.OnLoad);
                    var _faxImage = _tiffDecoder.Frames[0];
                    _faxImage.Freeze();

                    Fax_Image = _faxImage;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand testFax_OcrCommand;

        [Display(Description = "Hiermit wird die OCR-Engine gestestet und der Text aus der Bilddatei ausgelesen.")]
        public RelayCommand TestFax_OcrCommand
        {
            get
            {
                if (testFax_OcrCommand == null)
                    testFax_OcrCommand = new RelayCommand(() => OnTestFax_Ocr(), () => CanTestFax_Ocr());

                return testFax_OcrCommand;
            }
        }

        private bool CanTestFax_Ocr()
        {
            return true;
        }

        private void OnTestFax_Ocr()
        {
            try
            {
                Logger.WriteDebug("Settings: TestFax_OcrCommand");

                //Execute OCR
                var _faxtext = faxService.TestOcr(fax_ImagePath);

                var _textWindow = new TextWindow(_faxtext);
                _textWindow.Show();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand testFax_ExportSettings;

        [Display(Description = "Exportiert der Faxauswertung-Einstellungen")]
        public RelayCommand TestFax_ExportSettings
        {
            get
            {
                if (testFax_ExportSettings == null)
                    testFax_ExportSettings = new RelayCommand(() => OnTestFax_ExportSettings(),
                        () => CanTestFax_ExportSettings());

                return testFax_ExportSettings;
            }
        }

        private bool CanTestFax_ExportSettings()
        {
            return true;
        }

        private void OnTestFax_ExportSettings()
        {
            try
            {
                Logger.WriteDebug("Settings: TestFax_ExportSettings");

                var _saveFileDialog = new SaveFileDialog();
                _saveFileDialog.Filter = "Setting files (*.setting)|*.setting";
                _saveFileDialog.AddExtension = true;
                _saveFileDialog.CheckPathExists = true;
                _saveFileDialog.InitialDirectory = Environment.SpecialFolder.MyComputer.ToString();
                if (_saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Serializer.SerializeToFile(settingsOcr, _saveFileDialog.FileName);
                    MessageBox.Show("Einstellungsdatei wurden erfolgreich erstellt.", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show(
                    "Leider ist ein unerwarteter Fehler aufgetreten. Bitte setzen Sie sich mit dem Support in Verbindung.\r\n\r\n" +
                    ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private RelayCommand testFax_ImportSettings;

        [Display(Description = "Import der Faxauswertung-Einstellungen")]
        public RelayCommand TestFax_ImportSettings
        {
            get
            {
                if (testFax_ImportSettings == null)
                    testFax_ImportSettings = new RelayCommand(() => OnTestFax_ImportSettings(),
                        () => CanTestFax_ImportSettings());

                return testFax_ImportSettings;
            }
        }

        private bool CanTestFax_ImportSettings()
        {
            return true;
        }

        private void OnTestFax_ImportSettings()
        {
            try
            {
                Logger.WriteDebug("Settings: TestFax_ImportSettings");

                var _openFileDialog = new OpenFileDialog();
                _openFileDialog.Filter = "Setting files (*.setting)|*.setting";
                _openFileDialog.CheckFileExists = true;
                _openFileDialog.InitialDirectory = Environment.SpecialFolder.MyComputer.ToString();
                if (_openFileDialog.ShowDialog() != DialogResult.OK) return;

                var _settings = Serializer.DeserializeFromFile<SettingsOcr>(_openFileDialog.FileName);
                if (_settings == null)
                {
                    MessageBox.Show("Einstellungsdatei konnte nicht geladen werden", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                var _result =
                    MessageBox.Show(
                        "Einstellungsdatei wurden erfolgreich gelesen.\r\nWollen Sie Ihre aktuellen Einstellungen wirklich überschreiben?",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (_result != MessageBoxResult.Yes) return;

                settingsOcr = _settings;

                RaisePropertyChanged(() => Ocr_Absender);
                RaisePropertyChanged(() => Ocr_SchlagwortStart);
                RaisePropertyChanged(() => Ocr_SchlagwortStart_FirstWord);
                RaisePropertyChanged(() => Ocr_SchlagwortStop);
                RaisePropertyChanged(() => Ocr_Stichwort1Start);
                RaisePropertyChanged(() => Ocr_Stichwort1Start_FirstWord);
                RaisePropertyChanged(() => Ocr_Stichwort1Stop);
                RaisePropertyChanged(() => Ocr_Stichwort2Start);
                RaisePropertyChanged(() => Ocr_Stichwort2Start_FirstWord);
                RaisePropertyChanged(() => Ocr_Stichwort2Stop);
                RaisePropertyChanged(() => Ocr_Stichwort3Start);
                RaisePropertyChanged(() => Ocr_Stichwort3Start_FirstWord);
                RaisePropertyChanged(() => Ocr_Stichwort3Stop);
                RaisePropertyChanged(() => Ocr_OrtStart);
                RaisePropertyChanged(() => Ocr_OrtStart_FirstWord);
                RaisePropertyChanged(() => Ocr_OrtStop);
                RaisePropertyChanged(() => Ocr_StraßeStart);
                RaisePropertyChanged(() => Ocr_StraßeStart_FirstWord);
                RaisePropertyChanged(() => Ocr_StraßeStop);
                RaisePropertyChanged(() => Ocr_HausnummerStart);
                RaisePropertyChanged(() => Ocr_HausnummerStart_FirstWord);
                RaisePropertyChanged(() => Ocr_HausnummerStop);
                RaisePropertyChanged(() => Ocr_KoordinatenRWStart);
                RaisePropertyChanged(() => Ocr_KoordinatenRWStart_FirstWord);
                RaisePropertyChanged(() => Ocr_KoordinatenRWStop);
                RaisePropertyChanged(() => Ocr_KoordinatenHWStart);
                RaisePropertyChanged(() => Ocr_KoordinatenHWStart_FirstWord);
                RaisePropertyChanged(() => Ocr_KoordinatenHWStop);
                RaisePropertyChanged(() => Ocr_ObjektStart);
                RaisePropertyChanged(() => Ocr_ObjektStart_FirstWord);
                RaisePropertyChanged(() => Ocr_ObjektStop);
                RaisePropertyChanged(() => Ocr_StationStart);
                RaisePropertyChanged(() => Ocr_StationStart_FirstWord);
                RaisePropertyChanged(() => Ocr_StationStop);
                RaisePropertyChanged(() => Ocr_KreuzungStart);
                RaisePropertyChanged(() => Ocr_KreuzungStart_FirstWord);
                RaisePropertyChanged(() => Ocr_KreuzungStop);
                RaisePropertyChanged(() => Ocr_AbschnittStart);
                RaisePropertyChanged(() => Ocr_AbschnittStart_FirstWord);
                RaisePropertyChanged(() => Ocr_AbschnittStop);
                RaisePropertyChanged(() => Ocr_BemerkungStart);
                RaisePropertyChanged(() => Ocr_BemerkungStart_FirstWord);
                RaisePropertyChanged(() => Ocr_BemerkungStop);

                MessageBox.Show("Einstellungen wurden erfolgreich geladen.", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show(
                    "Leider ist ein unerwarteter Fehler aufgetreten. Bitte setzen Sie sich mit dem Support in Verbindung.\r\n\r\n" +
                    ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion //Fax

        #region Filter

        private RelayCommand addFilterCommand;

        [Display(Description = "Öffnet den Dialog zum Hinzufügen eines neuen Filter")]
        public RelayCommand AddFilterCommand
        {
            get
            {
                if (addFilterCommand == null)
                    addFilterCommand = new RelayCommand(() => OnAddFilter(), () => CanAddFilter());

                return addFilterCommand;
            }
        }

        private bool CanAddFilter()
        {
            return true;
        }

        private void OnAddFilter()
        {
            try
            {
                Logger.WriteDebug("Settings: AddFilterCommand");

                Messenger.Default.Send(new SettingsFilterDialog(business, 0));
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand<object> editFilterCommand;

        [Display(Description = "Öffnet den Dialog zum Hinzufügen eines neuen Filter")]
        public RelayCommand<object> EditFilterCommand
        {
            get
            {
                if (editFilterCommand == null)
                    editFilterCommand =
                        new RelayCommand<object>(param => OnEditFilter(param), param => CanEditFilter(param));

                return editFilterCommand;
            }
        }

        private bool CanEditFilter(object param)
        {
            if (param == null) return false;

            if (FilterList == null || FilterList.Count <= 0) return false;

            if (FilterList.Where(c => c.Id == ((Filter) param).Id).SingleOrDefault() == null) return false;

            return true;
        }

        private void OnEditFilter(object param)
        {
            try
            {
                Logger.WriteDebug("Settings: EditFilterCommand");

                Messenger.Default.Send(new SettingsFilterDialog(business, ((Filter) param).Id));
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand<object> deleteFilterCommand;

        [Display(Description = "Entfernt diesen Filter")]
        public RelayCommand<object> DeleteFilterCommand
        {
            get
            {
                if (deleteFilterCommand == null)
                    deleteFilterCommand =
                        new RelayCommand<object>(param => OnDeleteFilter(param), param => CanDeleteFilter(param));

                return deleteFilterCommand;
            }
        }

        private bool CanDeleteFilter(object param)
        {
            if (param == null) return false;

            if (FilterList == null || FilterList.Count <= 0) return false;

            if (FilterList.Where(c => c.Id == ((Filter) param).Id).SingleOrDefault() == null) return false;

            return true;
        }

        private void OnDeleteFilter(object param)
        {
            try
            {
                Logger.WriteDebug("Settings: DeleteFilterCommand");

                business.DeleteFilter((Filter) param);
                LoadFilterList();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion //Filter

        #region Aao

        private RelayCommand addAaoCommand;

        [Display(Description = "Öffnet den Dialog zum Hinzufügen einer neuen AAO")]
        public RelayCommand AddAaoCommand
        {
            get
            {
                if (addAaoCommand == null) addAaoCommand = new RelayCommand(() => OnAddAao(), () => CanAddAao());

                return addAaoCommand;
            }
        }

        private bool CanAddAao()
        {
            return true;
        }

        private void OnAddAao()
        {
            try
            {
                Logger.WriteDebug("Settings: AddAaoCommand");

                Messenger.Default.Send(new SettingsAaoDialog(business, 0));
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand<object> editAaoCommand;

        [Display(Description = "Öffnet den Dialog zum Bearbeiten der AAO")]
        public RelayCommand<object> EditAaoCommand
        {
            get
            {
                if (editAaoCommand == null)
                    editAaoCommand = new RelayCommand<object>(param => OnEditAao(param), param => CanEditAao(param));

                return editAaoCommand;
            }
        }

        private bool CanEditAao(object param)
        {
            if (param == null) return false;

            if (AaoList == null || AaoList.Count <= 0) return false;

            if (AaoList.Where(c => c.Id == ((Aao) param).Id).SingleOrDefault() == null) return false;

            return true;
        }

        private void OnEditAao(object param)
        {
            try
            {
                Logger.WriteDebug("Settings: EditAaoCommand");

                Messenger.Default.Send(new SettingsAaoDialog(business, ((Aao) param).Id));
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private RelayCommand<object> deleteAaoCommand;

        [Display(Description = "Entfernt diese AAO")]
        public RelayCommand<object> DeleteAaoCommand
        {
            get
            {
                if (deleteAaoCommand == null)
                    deleteAaoCommand =
                        new RelayCommand<object>(param => OnDeleteAao(param), param => CanDeleteAao(param));

                return deleteAaoCommand;
            }
        }

        private bool CanDeleteAao(object param)
        {
            if (param == null) return false;

            if (AaoList == null || AaoList.Count <= 0) return false;

            if (AaoList.Where(c => c.Id == ((Aao) param).Id).SingleOrDefault() == null) return false;

            return true;
        }

        private void OnDeleteAao(object param)
        {
            try
            {
                Logger.WriteDebug("Settings: DeleteAaoCommand");

                var _aaoId = ((Aao) param).Id;
                var _aao = business.GetAaoById(_aaoId);

                business.DeleteAao(_aao);
                LoadAaoList();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
                MessageBox.Show("Leider ist ein Fehler aufgetreten:\r\n" + ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion //Aao

        #endregion //Commands

        #region Events

        #endregion //Events

        #region Validation

        #endregion //Validation

        #region Public Properties

        #region Allgemein

        [Display(Description =
            "Dieser Ordner wird auf neue Alarmfaxe überwacht. In diesen Ordner muss die Empfangssoftware alle neuen Faxe speichern.")]
        public string Fax_PathInput
        {
            get => Settings.Default.Fax_PathInput;
            set
            {
                if (Settings.Default.Fax_PathInput == value) return;

                Settings.Default.Fax_PathInput = value;

                RaisePropertyChanged(() => Fax_PathInput);
            }
        }

        [Display(Description = "Nach dem Empfang und der Auswertung wird die Faxdatei in diesem Ordner archiviert")]
        public string Fax_PathArchive
        {
            get => Settings.Default.Fax_PathArchive;
            set
            {
                if (Settings.Default.Fax_PathArchive == value) return;

                Settings.Default.Fax_PathArchive = value;

                RaisePropertyChanged(() => Fax_PathArchive);
            }
        }

        [Display(Description = "Zeitdauer nach dem Empfang eines Faxes kein neues Fax akzeptiert wird")]
        public TimeSpan Fax_LockTime
        {
            get => Settings.Default.Fax_LockTime;
            set
            {
                if (Settings.Default.Fax_LockTime == value) return;

                Settings.Default.Fax_LockTime = value;

                RaisePropertyChanged(() => Fax_LockTime);
            }
        }
        
        [Display(Description = "IMAP-Server an den eine Einsatz-Mail gesendet wird. IP oder Hostname")]
        public string MailInput_Server
        {
            get => Settings.Default.MailInput_Server;
            set
            {
                if (Settings.Default.MailInput_Server == value) return;

                Settings.Default.MailInput_Server = value;

                RaisePropertyChanged(() => MailInput_Server);
            }
        }

        [Display(Description = "Aktivieren falls der Server eine SSL-Verschlüsselung benötigt")]
        public bool MailInput_SSL
        {
            get => Settings.Default.MailInput_SSL;
            set
            {
                if (Settings.Default.MailInput_SSL == value) return;

                Settings.Default.MailInput_SSL = value;

                RaisePropertyChanged(() => MailInput_SSL);
            }
        }

        [Display(Description = "Port des IMAP-Servers")]
        public int MailInput_Port
        {
            get => Settings.Default.MailInput_Port;
            set
            {
                if (Settings.Default.MailInput_Port == value) return;

                Settings.Default.MailInput_Port = value;

                RaisePropertyChanged(() => MailInput_Port);
            }
        }

        [Display(Description = "Benutzer zum Anmelden am IMAP-Server")]
        public string MailInput_User
        {
            get => Settings.Default.MailInput_User;
            set
            {
                if (Settings.Default.MailInput_User == value) return;

                Settings.Default.MailInput_User = value;

                RaisePropertyChanged(() => MailInput_User);
            }
        }

        [Display(Description = "Passwort des Benutzers zum Anmelden am IMAP-Server")]
        public string MailInput_Password
        {
            get => null;
            set { RaisePropertyChanged(() => MailInput_Password); }
        }

        [Display(Description =
            "Diese Zeichenfolge muss in jeder Einsatz-Mail vorkommen um als solche erkannt zu werden")]
        public string MailInput_Subject
        {
            get => Settings.Default.MailInput_Subject;
            set
            {
                if (Settings.Default.MailInput_Subject == value) return;

                Settings.Default.MailInput_Subject = value;

                RaisePropertyChanged(() => MailInput_Subject);
            }
        }

        #endregion //Allgemein

        #region Auswertung

        private string fax_ImagePath;
        private ImageSource fax_Image = new BitmapImage();

        public ImageSource Fax_Image
        {
            get => fax_Image;
            set
            {
                if (fax_Image == value) return;

                fax_Image = value;

                RaisePropertyChanged(() => Fax_Image);
            }
        }

        [Display(Description =
            "Dieser Text muss auf dem Fax vorkommen um es als Alarmfax auszuwerten. Beispiel: 'Leitstelle'\nWird kein Text eingegeben wird jedes Fax ausgewertet.")]
        public string Ocr_Absender
        {
            get => settingsOcr.Absender;
            set
            {
                if (settingsOcr.Absender == value) return;

                settingsOcr.Absender = value;

                RaisePropertyChanged(() => Ocr_Absender);
            }
        }

        [Display(Description = "Nach diesem Wort beginnt der Text des Schlagwortes.\nBeispiel: 'Schlagw.'")]
        public string Ocr_SchlagwortStart
        {
            get => settingsOcr.Schlagwort.Start;
            set
            {
                if (settingsOcr.Schlagwort.Start == value) return;

                settingsOcr.Schlagwort.Start = value;

                RaisePropertyChanged(() => Ocr_SchlagwortStart);
            }
        }

        [Display(Description = "Der Starttext wird nur akzeptiert, wenn es das erste Wort in der Zeile ist.")]
        public bool Ocr_SchlagwortStart_FirstWord
        {
            get => settingsOcr.Schlagwort.Start_FirstWord;
            set
            {
                if (settingsOcr.Schlagwort.Start_FirstWord == value) return;

                settingsOcr.Schlagwort.Start_FirstWord = value;

                RaisePropertyChanged(() => Ocr_SchlagwortStart_FirstWord);
            }
        }

        [Display(Description =
            "Text zwischen Start und diesem Wort wird als Schlagwort verwendet.\nWird kein Wort in dieses Feld eingetragen, wird der Text bis zum Zeilenende verwendet.")]
        public string Ocr_SchlagwortStop
        {
            get => settingsOcr.Schlagwort.Stop;
            set
            {
                if (settingsOcr.Schlagwort.Stop == value) return;

                settingsOcr.Schlagwort.Stop = value;

                RaisePropertyChanged(() => Ocr_SchlagwortStop);
            }
        }

        [Display(Description = "Nach diesem Wort beginnt der Text des Stichwort1.\nBeispiel: 'B:'")]
        public string Ocr_Stichwort1Start
        {
            get => settingsOcr.Stichwort1.Start;
            set
            {
                if (settingsOcr.Stichwort1.Start == value) return;

                settingsOcr.Stichwort1.Start = value;

                RaisePropertyChanged(() => Ocr_Stichwort1Start);
            }
        }

        [Display(Description = "Der Starttext wird nur akzeptiert, wenn es das erste Wort in der Zeile ist.")]
        public bool Ocr_Stichwort1Start_FirstWord
        {
            get => settingsOcr.Stichwort1.Start_FirstWord;
            set
            {
                if (settingsOcr.Stichwort1.Start_FirstWord == value) return;

                settingsOcr.Stichwort1.Start_FirstWord = value;

                RaisePropertyChanged(() => Ocr_Stichwort1Start_FirstWord);
            }
        }

        [Display(Description =
            "Text zwischen Start und diesem Wort wird als Stichwort1 verwendet.\nWird kein Wort in dieses Feld eingetragen, wird der Text bis zum Zeilenende verwendet.")]
        public string Ocr_Stichwort1Stop
        {
            get => settingsOcr.Stichwort1.Stop;
            set
            {
                if (settingsOcr.Stichwort1.Stop == value) return;

                settingsOcr.Stichwort1.Stop = value;

                RaisePropertyChanged(() => Ocr_Stichwort1Stop);
            }
        }

        [Display(Description = "Nach diesem Wort beginnt der Text des Stichwort2.\nBeispiel: 'THL:'")]
        public string Ocr_Stichwort2Start
        {
            get => settingsOcr.Stichwort2.Start;
            set
            {
                if (settingsOcr.Stichwort2.Start == value) return;

                settingsOcr.Stichwort2.Start = value;

                RaisePropertyChanged(() => Ocr_Stichwort2Start);
            }
        }

        [Display(Description = "Der Starttext wird nur akzeptiert, wenn es das erste Wort in der Zeile ist.")]
        public bool Ocr_Stichwort2Start_FirstWord
        {
            get => settingsOcr.Stichwort2.Start_FirstWord;
            set
            {
                if (settingsOcr.Stichwort2.Start_FirstWord == value) return;

                settingsOcr.Stichwort2.Start_FirstWord = value;

                RaisePropertyChanged(() => Ocr_Stichwort2Start_FirstWord);
            }
        }

        [Display(Description =
            "Text zwischen Start und diesem Wort wird als Stichwort2 verwendet.\nWird kein Wort in dieses Feld eingetragen, wird der Text bis zum Zeilenende verwendet.")]
        public string Ocr_Stichwort2Stop
        {
            get => settingsOcr.Stichwort2.Stop;
            set
            {
                if (settingsOcr.Stichwort2.Stop == value) return;

                settingsOcr.Stichwort2.Stop = value;

                RaisePropertyChanged(() => Ocr_Stichwort2Stop);
            }
        }

        [Display(Description = "Nach diesem Wort beginnt der Text des Stichwort3.\nBeispiel: 'RD:'")]
        public string Ocr_Stichwort3Start
        {
            get => settingsOcr.Stichwort3.Start;
            set
            {
                if (settingsOcr.Stichwort3.Start == value) return;

                settingsOcr.Stichwort3.Start = value;

                RaisePropertyChanged(() => Ocr_Stichwort3Start);
            }
        }

        [Display(Description = "Der Starttext wird nur akzeptiert, wenn es das erste Wort in der Zeile ist.")]
        public bool Ocr_Stichwort3Start_FirstWord
        {
            get => settingsOcr.Stichwort3.Start_FirstWord;
            set
            {
                if (settingsOcr.Stichwort3.Start_FirstWord == value) return;

                settingsOcr.Stichwort3.Start_FirstWord = value;

                RaisePropertyChanged(() => Ocr_Stichwort3Start_FirstWord);
            }
        }

        [Display(Description =
            "Text zwischen Start und diesem Wort wird als Stichwort3 verwendet.\nWird kein Wort in dieses Feld eingetragen, wird der Text bis zum Zeilenende verwendet.")]
        public string Ocr_Stichwort3Stop
        {
            get => settingsOcr.Stichwort3.Stop;
            set
            {
                if (settingsOcr.Stichwort3.Stop == value) return;

                settingsOcr.Stichwort3.Stop = value;

                RaisePropertyChanged(() => Ocr_Stichwort3Stop);
            }
        }

        [Display(Description = "Nach diesem Wort beginnt der Text des Ortes.\nBeispiel: 'Ort:'")]
        public string Ocr_OrtStart
        {
            get => settingsOcr.Ort.Start;
            set
            {
                if (settingsOcr.Ort.Start == value) return;

                settingsOcr.Ort.Start = value;

                RaisePropertyChanged(() => Ocr_OrtStart);
            }
        }

        [Display(Description = "Der Starttext wird nur akzeptiert, wenn es das erste Wort in der Zeile ist.")]
        public bool Ocr_OrtStart_FirstWord
        {
            get => settingsOcr.Ort.Start_FirstWord;
            set
            {
                if (settingsOcr.Ort.Start_FirstWord == value) return;

                settingsOcr.Ort.Start_FirstWord = value;

                RaisePropertyChanged(() => Ocr_OrtStart_FirstWord);
            }
        }

        [Display(Description =
            "Text zwischen Start und diesem Wort wird als Ort verwendet. Wenn möglich sollte nur Postleitzahl und der Ort ohne Ortsteile verwendet werden.\nWird kein Wort in dieses Feld eingetragen, wird der Text bis zum Zeilenende verwendet.")]
        public string Ocr_OrtStop
        {
            get => settingsOcr.Ort.Stop;
            set
            {
                if (settingsOcr.Ort.Stop == value) return;

                settingsOcr.Ort.Stop = value;

                RaisePropertyChanged(() => Ocr_OrtStop);
            }
        }

        [Display(Description = "Nach diesem Wort beginnt der Text der Straße.\nBeispiel: 'Straße:'")]
        public string Ocr_StraßeStart
        {
            get => settingsOcr.Straße.Start;
            set
            {
                if (settingsOcr.Straße.Start == value) return;

                settingsOcr.Straße.Start = value;

                RaisePropertyChanged(() => Ocr_StraßeStart);
            }
        }

        [Display(Description = "Der Starttext wird nur akzeptiert, wenn es das erste Wort in der Zeile ist.")]
        public bool Ocr_StraßeStart_FirstWord
        {
            get => settingsOcr.Straße.Start_FirstWord;
            set
            {
                if (settingsOcr.Straße.Start_FirstWord == value) return;

                settingsOcr.Straße.Start_FirstWord = value;

                RaisePropertyChanged(() => Ocr_StraßeStart_FirstWord);
            }
        }

        [Display(Description =
            "Text zwischen Start und diesem Wort wird als Straße verwendet.\nWird kein Wort in dieses Feld eingetragen, wird der Text bis zum Zeilenende verwendet.")]
        public string Ocr_StraßeStop
        {
            get => settingsOcr.Straße.Stop;
            set
            {
                if (settingsOcr.Straße.Stop == value) return;

                settingsOcr.Straße.Stop = value;

                RaisePropertyChanged(() => Ocr_StraßeStop);
            }
        }

        [Display(Description = "Nach diesem Wort beginnt der Text der Hausnummer.\nBeispiel: 'Nr.'")]
        public string Ocr_HausnummerStart
        {
            get => settingsOcr.Hausnummer.Start;
            set
            {
                if (settingsOcr.Hausnummer.Start == value) return;

                settingsOcr.Hausnummer.Start = value;

                RaisePropertyChanged(() => Ocr_HausnummerStart);
            }
        }

        [Display(Description = "Der Starttext wird nur akzeptiert, wenn es das erste Wort in der Zeile ist.")]
        public bool Ocr_HausnummerStart_FirstWord
        {
            get => settingsOcr.Hausnummer.Start_FirstWord;
            set
            {
                if (settingsOcr.Hausnummer.Start_FirstWord == value) return;

                settingsOcr.Hausnummer.Start_FirstWord = value;

                RaisePropertyChanged(() => Ocr_HausnummerStart_FirstWord);
            }
        }

        [Display(Description =
            "Text zwischen Start und diesem Wort wird als Hausnummer verwendet.\nWird kein Wort in dieses Feld eingetragen, wird der Text bis zum Zeilenende verwendet.")]
        public string Ocr_HausnummerStop
        {
            get => settingsOcr.Hausnummer.Stop;
            set
            {
                if (settingsOcr.Hausnummer.Stop == value) return;

                settingsOcr.Hausnummer.Stop = value;

                RaisePropertyChanged(() => Ocr_HausnummerStop);
            }
        }

        [Display(Description = "Nach diesem Wort beginnt der Text der Koordinaten-Rechtswert.\nBeispiel: 'Nr.'")]
        public string Ocr_KoordinatenRWStart
        {
            get => settingsOcr.KoordinatenRW.Start;
            set
            {
                if (settingsOcr.KoordinatenRW.Start == value) return;

                settingsOcr.KoordinatenRW.Start = value;

                RaisePropertyChanged(() => Ocr_KoordinatenRWStart);
            }
        }

        [Display(Description = "Der Starttext wird nur akzeptiert, wenn es das erste Wort in der Zeile ist.")]
        public bool Ocr_KoordinatenRWStart_FirstWord
        {
            get => settingsOcr.KoordinatenRW.Start_FirstWord;
            set
            {
                if (settingsOcr.KoordinatenRW.Start_FirstWord == value) return;

                settingsOcr.KoordinatenRW.Start_FirstWord = value;

                RaisePropertyChanged(() => Ocr_KoordinatenRWStart_FirstWord);
            }
        }

        [Display(Description =
            "Text zwischen Start und diesem Wort wird als Koordinaten-Rechtswert verwendet.\nWird kein Wort in dieses Feld eingetragen, wird der Text bis zum Zeilenende verwendet.")]
        public string Ocr_KoordinatenRWStop
        {
            get => settingsOcr.KoordinatenRW.Stop;
            set
            {
                if (settingsOcr.KoordinatenRW.Stop == value) return;

                settingsOcr.KoordinatenRW.Stop = value;

                RaisePropertyChanged(() => Ocr_KoordinatenRWStop);
            }
        }

        [Display(Description = "Nach diesem Wort beginnt der Text der Koordinaten-Hochwert.\nBeispiel: 'Nr.'")]
        public string Ocr_KoordinatenHWStart
        {
            get => settingsOcr.KoordinatenHW.Start;
            set
            {
                if (settingsOcr.KoordinatenHW.Start == value) return;

                settingsOcr.KoordinatenHW.Start = value;

                RaisePropertyChanged(() => Ocr_KoordinatenHWStart);
            }
        }

        [Display(Description = "Der Starttext wird nur akzeptiert, wenn es das erste Wort in der Zeile ist.")]
        public bool Ocr_KoordinatenHWStart_FirstWord
        {
            get => settingsOcr.KoordinatenHW.Start_FirstWord;
            set
            {
                if (settingsOcr.KoordinatenHW.Start_FirstWord == value) return;

                settingsOcr.KoordinatenHW.Start_FirstWord = value;

                RaisePropertyChanged(() => Ocr_KoordinatenHWStart_FirstWord);
            }
        }

        [Display(Description =
            "Text zwischen Start und diesem Wort wird als Koordinaten-Hochwert verwendet.\nWird kein Wort in dieses Feld eingetragen, wird der Text bis zum Zeilenende verwendet.")]
        public string Ocr_KoordinatenHWStop
        {
            get => settingsOcr.KoordinatenHW.Stop;
            set
            {
                if (settingsOcr.KoordinatenHW.Stop == value) return;

                settingsOcr.KoordinatenHW.Stop = value;

                RaisePropertyChanged(() => Ocr_KoordinatenHWStop);
            }
        }

        [Display(Description = "Nach diesem Wort beginnt der Text des Objekts.\nBeispiel: 'Objekt:'")]
        public string Ocr_ObjektStart
        {
            get => settingsOcr.Objekt.Start;
            set
            {
                if (settingsOcr.Objekt.Start == value) return;

                settingsOcr.Objekt.Start = value;

                RaisePropertyChanged(() => Ocr_ObjektStart);
            }
        }

        [Display(Description = "Der Starttext wird nur akzeptiert, wenn es das erste Wort in der Zeile ist.")]
        public bool Ocr_ObjektStart_FirstWord
        {
            get => settingsOcr.Objekt.Start_FirstWord;
            set
            {
                if (settingsOcr.Objekt.Start_FirstWord == value) return;

                settingsOcr.Objekt.Start_FirstWord = value;

                RaisePropertyChanged(() => Ocr_ObjektStart_FirstWord);
            }
        }

        [Display(Description =
            "Text zwischen Start und diesem Wort wird als Objekt verwendet.\nWird kein Wort in dieses Feld eingetragen, wird der Text bis zum Zeilenende verwendet.")]
        public string Ocr_ObjektStop
        {
            get => settingsOcr.Objekt.Stop;
            set
            {
                if (settingsOcr.Objekt.Stop == value) return;

                settingsOcr.Objekt.Stop = value;

                RaisePropertyChanged(() => Ocr_ObjektStop);
            }
        }

        [Display(Description = "Nach diesem Wort beginnt der Text der Station.\nBeispiel: 'Station:'")]
        public string Ocr_StationStart
        {
            get => settingsOcr.Station.Start;
            set
            {
                if (settingsOcr.Station.Start == value) return;

                settingsOcr.Station.Start = value;

                RaisePropertyChanged(() => Ocr_StationStart);
            }
        }

        [Display(Description = "Der Starttext wird nur akzeptiert, wenn es das erste Wort in der Zeile ist.")]
        public bool Ocr_StationStart_FirstWord
        {
            get => settingsOcr.Station.Start_FirstWord;
            set
            {
                if (settingsOcr.Station.Start_FirstWord == value) return;

                settingsOcr.Station.Start_FirstWord = value;

                RaisePropertyChanged(() => Ocr_StationStart_FirstWord);
            }
        }

        [Display(Description =
            "Text zwischen Start und diesem Wort wird als Station verwendet.\nWird kein Wort in dieses Feld eingetragen, wird der Text bis zum Zeilenende verwendet.")]
        public string Ocr_StationStop
        {
            get => settingsOcr.Station.Stop;
            set
            {
                if (settingsOcr.Station.Stop == value) return;

                settingsOcr.Station.Stop = value;

                RaisePropertyChanged(() => Ocr_StationStop);
            }
        }


        [Display(Description = "Nach diesem Wort beginnt der Text der Kreuzung.\nBeispiel: 'Kreuzung:'")]
        public string Ocr_KreuzungStart
        {
            get => settingsOcr.Kreuzung.Start;
            set
            {
                if (settingsOcr.Kreuzung.Start == value) return;

                settingsOcr.Kreuzung.Start = value;

                RaisePropertyChanged(() => Ocr_KreuzungStart);
            }
        }

        [Display(Description = "Der Starttext wird nur akzeptiert, wenn es das erste Wort in der Zeile ist.")]
        public bool Ocr_KreuzungStart_FirstWord
        {
            get => settingsOcr.Kreuzung.Start_FirstWord;
            set
            {
                if (settingsOcr.Kreuzung.Start_FirstWord == value) return;

                settingsOcr.Kreuzung.Start_FirstWord = value;

                RaisePropertyChanged(() => Ocr_KreuzungStart_FirstWord);
            }
        }

        [Display(Description =
            "Text zwischen Start und diesem Wort wird als Kreuzung verwendet.\nWird kein Wort in dieses Feld eingetragen, wird der Text bis zum Zeilenende verwendet.")]
        public string Ocr_KreuzungStop
        {
            get => settingsOcr.Kreuzung.Stop;
            set
            {
                if (settingsOcr.Kreuzung.Stop == value) return;

                settingsOcr.Kreuzung.Stop = value;

                RaisePropertyChanged(() => Ocr_KreuzungStop);
            }
        }


        [Display(Description = "Nach diesem Wort beginnt der Text des Abschnitts.\nBeispiel: 'Abschnitt:'")]
        public string Ocr_AbschnittStart
        {
            get => settingsOcr.Abschnitt.Start;
            set
            {
                if (settingsOcr.Abschnitt.Start == value) return;

                settingsOcr.Abschnitt.Start = value;

                RaisePropertyChanged(() => Ocr_AbschnittStart);
            }
        }

        [Display(Description = "Der Starttext wird nur akzeptiert, wenn es das erste Wort in der Zeile ist.")]
        public bool Ocr_AbschnittStart_FirstWord
        {
            get => settingsOcr.Abschnitt.Start_FirstWord;
            set
            {
                if (settingsOcr.Abschnitt.Start_FirstWord == value) return;

                settingsOcr.Abschnitt.Start_FirstWord = value;

                RaisePropertyChanged(() => Ocr_AbschnittStart_FirstWord);
            }
        }

        [Display(Description =
            "Text zwischen Start und diesem Wort wird als Abschnitt verwendet.\nWird kein Wort in dieses Feld eingetragen, wird der Text bis zum Zeilenende verwendet.")]
        public string Ocr_AbschnittStop
        {
            get => settingsOcr.Abschnitt.Stop;
            set
            {
                if (settingsOcr.Abschnitt.Stop == value) return;

                settingsOcr.Abschnitt.Stop = value;

                RaisePropertyChanged(() => Ocr_AbschnittStop);
            }
        }

        [Display(Description = "Nach diesem Wort beginnt der Text der Bemerkung.\nBeispiel: 'Bemerkung'")]
        public string Ocr_BemerkungStart
        {
            get => settingsOcr.Bemerkung.Start;
            set
            {
                if (settingsOcr.Bemerkung.Start == value) return;

                settingsOcr.Bemerkung.Start = value;

                RaisePropertyChanged(() => Ocr_BemerkungStart);
            }
        }

        [Display(Description = "Der Starttext wird nur akzeptiert, wenn es das erste Wort in der Zeile ist.")]
        public bool Ocr_BemerkungStart_FirstWord
        {
            get => settingsOcr.Bemerkung.Start_FirstWord;
            set
            {
                if (settingsOcr.Bemerkung.Start_FirstWord == value) return;

                settingsOcr.Bemerkung.Start_FirstWord = value;

                RaisePropertyChanged(() => Ocr_BemerkungStart_FirstWord);
            }
        }

        [Display(Description =
            "Text zwischen Start und diesem Wort wird als Bemerkung verwendet.\nWird kein Wort in dieses Feld eingetragen, wird der Text bis zum Faxende verwendet.")]
        public string Ocr_BemerkungStop
        {
            get => settingsOcr.Bemerkung.Stop;
            set
            {
                if (settingsOcr.Bemerkung.Stop == value) return;

                settingsOcr.Bemerkung.Stop = value;

                RaisePropertyChanged(() => Ocr_BemerkungStop);
            }
        }

        #endregion //Auswertung 

        #region Filter

        private ObservableCollection<Filter> filterList;

        public ObservableCollection<Filter> FilterList
        {
            get => filterList;
            set
            {
                if (filterList == value) return;

                filterList = value;

                RaisePropertyChanged(() => FilterList);
            }
        }

        #endregion //Filter

        #region AAO

        private ObservableCollection<Aao> aaoList;

        public ObservableCollection<Aao> AaoList
        {
            get => aaoList;
            set
            {
                if (aaoList == value) return;

                aaoList = value;

                RaisePropertyChanged(() => AaoList);
            }
        }

        #endregion //AAO           

        #endregion //Public Properties

        #region Private Functions

        private void LoadAaoList()
        {
            aaoList = new ObservableCollection<Aao>(business.GetAaoOverviewAsync().Result);
            RaisePropertyChanged(() => AaoList);
        }

        private void LoadFilterList()
        {
            filterList = new ObservableCollection<Filter>(business.GetAllFilterAsync().Result);
            RaisePropertyChanged(() => FilterList);
        }

        #endregion //Private Funtions
    }
}