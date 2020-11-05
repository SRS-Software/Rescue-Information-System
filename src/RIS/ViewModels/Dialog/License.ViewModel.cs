//*****************************************************************************
//* Copyright (c) All Right Reserved             
//*****************************************************************************
//* Company      : SpecialRescueSolutions UG(haftungsbeschränkt)   
//* Web          : http://www.srs-software.de
//* Project      : RIS
//* Classname    : License.ViewModel
//* Language     : C#
//* Author       : Scholz Flo            
//* Contact info : kontakt@srs-software.de
//*****************************************************************************

using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using LogicNP.CryptoLicensing;
using Microsoft.Practices.ServiceLocation;


namespace RIS.ViewModels
{
    public class LicenseViewModel : ViewModelBase
    {
        private readonly CryptoLicense cryptoLicense;
        public LicenseViewModel(UserControl _userControl, CryptoLicense _cryptoLicense)
        {
            currentUserControl = _userControl;
            cryptoLicense = _cryptoLicense;

            //Check cryptoLicense status
            if (!cryptoLicense.ValidateSignature())
            {
                this.LicenseStatusText = string.Format("Die Lizenz ist  ungültig!\nUm die Software weiter zu verwenden, wird ein gültiger Lizenzkey benötigt.", cryptoLicense.Status);
            }
            else if (cryptoLicense.IsLicenseDisabledInService())
            {
                this.LicenseStatusText = "Diese Lizenz wurde deaktiviert!\nUm die Software weiter zu verwenden, wird ein gültiger Lizenzkey benötigt.";
            }
            else if (cryptoLicense.IsEvaluationExpired() || DateTime.Now > cryptoLicense.DateExpires)
            {
                this.LicenseStatusText = "Der Testzeitraum ist leider abgelaufen!\nUm die Software weiter zu verwenden, wird ein gültiger Lizenzkey benötigt.";
            }
            else if (cryptoLicense.IsEvaluationLicense())
            {
                this.IsLicenseValid = true;
                this.LicenseStatusText = "Vielen Dank, dass Sie diese Software testen!";

                if (cryptoLicense.HasDateExpires)
                {
                    TimeSpan ts = (cryptoLicense.DateExpires.Date - DateTime.Now.Date);
                    int remaining = ts.Days;
                    if (remaining < 0)
                        remaining = 0;
                    int max = (cryptoLicense.DateExpires.Date - cryptoLicense.DateGenerated.Date).Days;
                    if (max < 0)
                        max = 0;
                    this.EvaluationProgressMax = max;
                    this.EvaluationProgressValue = remaining;
                    this.EvaluationProgressText = string.Format("Die Lizenz läuft am {0} aus! {1} Tage verbleibend", cryptoLicense.DateExpires.ToString("dd-MMM-yyyy"), remaining);
                }
                if (cryptoLicense.HasMaxUsageDays)
                {
                    int remaining = cryptoLicense.MaxUsageDays - cryptoLicense.CurrentUsageDays;
                    if (remaining < 0)
                        remaining = 0;
                    else
                        remaining++;

                    this.EvaluationProgressMax = cryptoLicense.MaxUsageDays;
                    this.EvaluationProgressValue = remaining;
                    this.EvaluationProgressText = string.Format("{0} von {1} Tagen verbleibend", remaining, cryptoLicense.MaxUsageDays);
                }
                if (cryptoLicense.HasMaxUniqueUsageDays)
                {
                    int remaining = cryptoLicense.MaxUniqueUsageDays - cryptoLicense.CurrentUniqueUsageDays;
                    if (remaining < 0)
                        remaining = 0;
                    else if (remaining < cryptoLicense.MaxUniqueUsageDays)
                        remaining++;

                    this.EvaluationProgressMax = cryptoLicense.MaxUsageDays;
                    this.EvaluationProgressValue = remaining;
                    this.EvaluationProgressText = string.Format("{0} von {1} kompletten Nutzungstagen verbleibend", remaining, cryptoLicense.MaxUniqueUsageDays);
                }
                if (cryptoLicense.HasMaxExecutions)
                {
                    int remaining = cryptoLicense.MaxExecutions - cryptoLicense.CurrentExecutions;
                    if (remaining < 0)
                        remaining = 0;
                    else if (remaining < cryptoLicense.MaxExecutions)
                        remaining++;

                    this.EvaluationProgressMax = cryptoLicense.MaxExecutions;
                    this.EvaluationProgressValue = remaining;
                    this.EvaluationProgressText = string.Format("{0} von {1} Programmstarts verbleibend", remaining, cryptoLicense.MaxExecutions);
                }
            }
            else
            {
                this.LicenseStatusText = string.Format("Die Lizenz ist  ungültig:  {0}\nUm die Software weiter zu verwenden, wird ein gültiger Lizenzkey benötigt.", cryptoLicense.Status);
            }
        }


        #region Commands
        private RelayCommand closeCommand;
        public RelayCommand CloseCommand
        {
            get
            {
                if (closeCommand == null)
                    closeCommand = new RelayCommand(() => this.OnClose(), () => this.CanClose());
                return closeCommand;
            }
        }
        private bool CanClose()
        {
            return true;
        }
        private void OnClose()
        {
            SRS.Utilities.Logger.Instance.WriteDebug("LicenseWindow: CloseCommand");

            this.RaiseCloseRequestEvent();
        }     
                      
        #region LicenseEvaluation
        private RelayCommand continueEvaluation;
        public RelayCommand ContinueEvaluation
        {
            get
            {
                if (continueEvaluation == null)
                {
                    continueEvaluation = new RelayCommand(
                        () => OnContinueEvaluation(),
                        () => CanContinueEvaluation()
                    );
                }
                return continueEvaluation;
            }
        }
        private void OnContinueEvaluation()
        {
            this.RaiseCloseRequestEvent();
        }
        private bool CanContinueEvaluation()
        {
            return this.IsLicenseValid;
        }   
 
        private RelayCommand inputLicense;
        public RelayCommand InputLicense
        {
            get
            {
                if (inputLicense == null)
                {
                    inputLicense = new RelayCommand(
                        () => OnInputLicense(),
                        () => CanInputLicense()
                    );
                }
                return inputLicense;
            }
        }
        private void OnInputLicense()
        {
            this.CurrentUserControl = new Views.LicenseInputUserControl();
        }
        private bool CanInputLicense()
        {
            return true;
        }        

        private RelayCommand purchase;
        public RelayCommand Purchase
        {
            get
            {
                if (purchase == null)
                {
                    purchase = new RelayCommand(
                        () => OnPurchase(),
                        () => CanPurchase()
                    );
                }
                return purchase;
            }
        }
        private void OnPurchase()
        {
            ProcessStartInfo pi = new ProcessStartInfo("http://srs-software.de", string.Empty);
            pi.UseShellExecute = true;
            pi.Verb = "open";
            pi.WindowStyle = ProcessWindowStyle.Maximized;
            System.Diagnostics.Process.Start(pi);

            Environment.Exit(0);
        }
        private bool CanPurchase()
        {
            return true;
        }
        #endregion //LicenseEvaluation

        #region LicenseInput 
        private RelayCommand pasteClipboard;
        public RelayCommand PasteClipboard
        {
            get
            {
                if (pasteClipboard == null)
                {
                    pasteClipboard = new RelayCommand(
                        () => OnPasteClipboard(),
                        () => CanPasteClipboard()
                    );
                }
                return pasteClipboard;
            }
        }
        private void OnPasteClipboard()
        {
            System.Windows.Forms.IDataObject dataObj = System.Windows.Forms.Clipboard.GetDataObject();
            this.LicenseInput_Key = dataObj.GetData(System.Windows.Forms.DataFormats.Text, true) as string;
        }
        private bool CanPasteClipboard()
        {
            return true;
        }
           
        private RelayCommand pasteFile;
        public RelayCommand PasteFile
        {
            get
            {
                if (pasteFile == null)
                {
                    pasteFile = new RelayCommand(
                        () => OnPasteFile(),
                        () => CanPasteFile()
                    );
                }
                return pasteFile;
            }
        }
        private void OnPasteFile()
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Filter = "All Files (*.*)|*.*";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (System.IO.StreamReader sr = System.IO.File.OpenText(dlg.FileName))
                {
                    this.LicenseInput_Key = sr.ReadToEnd();
                }
            }
        }
        private bool CanPasteFile()
        {
            return true;
        }
             
        private RelayCommand checkKey;
        public RelayCommand CheckKey
        {
            get
            {
                if (checkKey == null)
                {
                    checkKey = new RelayCommand(
                        () => OnCheckKey(),
                        () => CanCheckKey()
                    );
                }
                return checkKey;
            }
        }
        private void OnCheckKey()
        {
            try
            {
                //Show wait screen
                ServiceLocator.Current.GetInstance<RIS.Views.WaitSplashScreen>().Show();

                // Create temp cryptoLicense object
                CryptoLicense _temp = Activator.CreateInstance(cryptoLicense.GetType()) as CryptoLicense;
                _temp.StorageMode = cryptoLicense.StorageMode;
                _temp.ValidationKey = cryptoLicense.ValidationKey;
                _temp.LicenseServiceURL = cryptoLicense.LicenseServiceURL;
                _temp.HostAssembly = cryptoLicense.HostAssembly;
                _temp.LicenseServiceSettingsFilePath = cryptoLicense.LicenseServiceSettingsFilePath;
                _temp.RegistryStoragePath = cryptoLicense.RegistryStoragePath;
                _temp.FileStoragePath = cryptoLicense.FileStoragePath;

                //Check if internet connection to cryptoLicenseService is working
                if (_temp.PingLicenseService() != null)
                {   
                    Xceed.Wpf.Toolkit.MessageBox.Show("Leider ist der Lizenzserver nicht erreichbar.\nUm Ihre Lizenz zu aktivieren, wird eine funktionierende Internetverbindung benötigt.", "RescueInformationSystem - ERROR", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);        
                    return;
                }

                // See if its a serial
                SerialValidationResult result = _temp.GetLicenseFromSerial(this.LicenseInput_Key, string.Empty);
                if (result == SerialValidationResult.Failed)
                {
                    // Its a serial, but is invalid
                    string str = "Serial Überprüfung fehlgeschlagen: ";
                    Exception ex = _temp.GetStatusException(LicenseStatus.SerialCodeInvalid);
                    if (ex != null) // Additional info available for the status
                        str += ex.Message;
                    else
                        str += "<no additional information>";

                    Xceed.Wpf.Toolkit.MessageBox.Show("Es ist ein Fehler aufgetreten: " +str, "RescueInformationSystem - ERROR", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }

                // Not a serial, set .LicenseCode property
                if (result == SerialValidationResult.NotASerial)
                    _temp.LicenseCode = this.LicenseInput_Key;

                // Validate cryptoLicense
                if (_temp.Status == LicenseStatus.Valid)
                {
                    // Valid , dispose old cryptoLicense, replace with 'temp' cryptoLicense
                    cryptoLicense.Dispose();
                }

                //Close wait screen
                ServiceLocator.Current.GetInstance<RIS.Views.WaitSplashScreen>().Close();

                //Show result
                if (_temp.Status != LicenseStatus.Valid)
                {
                    string additional = "Error code: " + _temp.Status.ToString();
                    Exception ex = _temp.GetStatusException(_temp.Status);
                    if (ex != null)
                        additional += ". Error message: " + ex.Message;

                    Xceed.Wpf.Toolkit.MessageBox.Show("Die eingegebene Lizenz war leider ungültig. Bitte geben Sie einen gültigen Lizenzkey ein und versuchen es erneut.\n" + additional, "RescueInformationSystem - ERROR", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);  
                    return;
                }
                else
                {
                    cryptoLicense.Save();
                    this.IsLicenseValid = true;
                    this.ParseLicenseInfosForThanks();
                    this.CurrentUserControl = new Views.LicenseThanksUserControl();
                }
            }
            catch (Exception ex)
            {
                SRS.Utilities.Logger.Instance.WriteError(System.Reflection.MethodBase.GetCurrentMethod(), ex);
            }
            finally
            {
                ServiceLocator.Current.GetInstance<RIS.Views.WaitSplashScreen>().Close();
            }
        }
        private bool CanCheckKey()
        {
            //if (string.IsNullOrEmpty(this.LicenseInput_Key))
            //    return false;

            return true;    
        }
        #endregion //LicenseInput

        #region LicenseTrial     
        private RelayCommand getTrialLicense;
        public RelayCommand GetTrialLicense
        {
            get
            {
                if (getTrialLicense == null)
                {
                    getTrialLicense = new RelayCommand(
                        () => OnGetTrialLicense(),
                        () => CanGetTrialLicense()
                    );
                }
                return getTrialLicense;
            }
        }
        private void OnGetTrialLicense()
        {
            //Get new 30 day cryptoLicense from server 
            string userData = "Organisation=" + this.LicenseTrial_Organisation + " (Evaluation-Version)";
            userData += "#Name=" + this.LicenseTrial_Name;
            userData += "#Mail=" + this.LicenseTrial_Mail;
            userData += "#Liteversion=False";

            if (cryptoLicense.GetLicenseFromProfile("Trial", userData))
            {
                cryptoLicense.Save();
                this.IsLicenseValid = true;
                this.ParseLicenseInfosForThanks();
                this.CurrentUserControl = new Views.LicenseThanksUserControl();
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Fehler beim Verbindungsaufbau zum Lizenzserver!", "RescueInformationSystem - ERROR", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                Environment.Exit(0);
                return;
            }
        }
        private bool CanGetTrialLicense()
        {
            if (string.IsNullOrEmpty(this.LicenseTrial_Organisation) ||
                this.LicenseTrial_Organisation.Length < 5 ||
                string.IsNullOrEmpty(this.LicenseTrial_Name) ||
                this.LicenseTrial_Name.Length < 5 ||
                string.IsNullOrEmpty(this.LicenseTrial_Mail) ||
                this.LicenseTrial_Mail.Length < 5)
                return false;

            return true;
        }
        
            
        #endregion //LicenseTrial

        #endregion //Commands


        #region Events
        public event EventHandler CloseRequested;
        private void RaiseCloseRequestEvent()
        {
            EventHandler handler = this.CloseRequested;
            if (handler != null)
                handler(this, new EventArgs());
        }
        #endregion //Public Funtions

        #region   Public Properties
        public string WindowTitel
        {
            get
            {
                return "License";
            }
        }             
        private UserControl currentUserControl;
        public UserControl CurrentUserControl
        {
            get
            {
                return currentUserControl;
            }
            set
            {
                if (currentUserControl == value)
                    return;

                currentUserControl = value;
                RaisePropertyChanged(() => CurrentUserControl);
            }
        }

        public bool IsLicenseValid { get; set; }

        #region LicenseEvaluation    
        private string cryptoLicenseStatusText = "Vielen Dank, dass Sie diese Software testen!";
        public string LicenseStatusText
        {
            get
            {
                return cryptoLicenseStatusText;
            }
            set
            {
                if (cryptoLicenseStatusText == value)
                    return;

                cryptoLicenseStatusText = value;
                RaisePropertyChanged(() => LicenseStatusText);
            }
        }
        private int evaluationProgressMax;
        public int EvaluationProgressMax
        {
            get
            {
                return evaluationProgressMax;
            }
            set
            {
                if (evaluationProgressMax == value)
                    return;

                evaluationProgressMax = value;
                RaisePropertyChanged(() => EvaluationProgressMax);
            }
        }          
        private int evaluationProgressValue;
        public int EvaluationProgressValue
        {
            get
            {
                return evaluationProgressValue;
            }
            set
            {
                if (evaluationProgressValue == value)
                    return;

                evaluationProgressValue = value;
                RaisePropertyChanged(() => EvaluationProgressValue);
            }
        }
        private string evaluationProgressText;
        public string EvaluationProgressText
        {
            get
            {
                return evaluationProgressText;
            }
            set
            {
                if (evaluationProgressText == value)
                    return;

                evaluationProgressText = value;
                RaisePropertyChanged(() => EvaluationProgressText);
            }
        }         
        #endregion //LicenseEvaluation

        #region LicenseInput      
        private string cryptoLicenseInput_Key;
        public string LicenseInput_Key
        {
            get
            {
                return cryptoLicenseInput_Key;
            }
            set
            {
                if (cryptoLicenseInput_Key == value)
                    return;

                cryptoLicenseInput_Key = value;
                RaisePropertyChanged(() => LicenseInput_Key);
            }
        }                           
        #endregion //LicenseInput

        #region LicenseTrial      
        private string cryptoLicenseTrial_Organisation;
        public string LicenseTrial_Organisation
        {
            get
            {
                return cryptoLicenseTrial_Organisation;
            }
            set
            {
                if (cryptoLicenseTrial_Organisation == value)
                    return;

                cryptoLicenseTrial_Organisation = value;
                RaisePropertyChanged(() => LicenseTrial_Organisation);
            }
        }
        private string cryptoLicenseTrial_Name;
        public string LicenseTrial_Name
        {
            get
            {
                return cryptoLicenseTrial_Name;
            }
            set
            {
                if (cryptoLicenseTrial_Name == value)
                    return;

                cryptoLicenseTrial_Name = value;
                RaisePropertyChanged(() => LicenseTrial_Name);
            }
        }
        private string cryptoLicenseTrial_Mail;
        public string LicenseTrial_Mail
        {
            get
            {
                return cryptoLicenseTrial_Mail;
            }
            set
            {
                if (cryptoLicenseTrial_Mail == value)
                    return;

                cryptoLicenseTrial_Mail = value;
                RaisePropertyChanged(() => LicenseTrial_Mail);
            }
        }           
        #endregion //LicenseTrial

        #region LicenseThanks   

        public string LicenseThanks_HardwareID { get; set; }
        public string LicenseThanks_Organisation { get; set; }
        public string LicenseThanks_Name { get; set; }
        public string LicenseThanks_Mail { get; set; }
        public string LicenseThanks_Version { get; set; }
        public string LicenseThanks_DateExpires { get; set; }          

        public bool LicenseThanks_FeatureMailOn { get; set; }
        public BitmapImage LicenseThanks_FeatureMailImage
        {
            get
            {       
                if (this.LicenseThanks_FeatureMailOn)
                    return new BitmapImage(new Uri("/RIS;component/Images/Image.Ok.png", UriKind.Relative));
                else
                    return new BitmapImage(new Uri("/RIS;component/Images/Image.Block.png", UriKind.Relative));
            }
        }

        public bool LicenseThanks_FeatureMapOn { get; set; }
        public BitmapImage LicenseThanks_FeatureMapImage
        {
            get
            {
                if (this.LicenseThanks_FeatureMapOn)
                    return new BitmapImage(new Uri("/RIS;component/Images/Image.Ok.png", UriKind.Relative));
                else
                    return new BitmapImage(new Uri("/RIS;component/Images/Image.Block.png", UriKind.Relative));
            }
        }

        public bool LicenseThanks_FeatureAlarmappOn { get; set; }
        public BitmapImage LicenseThanks_FeatureAlarmappImage
        {
            get
            {
                if (this.LicenseThanks_FeatureMapOn)
                    return new BitmapImage(new Uri("/RIS;component/Images/Image.Ok.png", UriKind.Relative));
                else
                    return new BitmapImage(new Uri("/RIS;component/Images/Image.Block.png", UriKind.Relative));
            }
        }
        #endregion //LicenseThanks 

        #endregion //Public Properties


        #region Public Functions 
        #endregion //Public Funtions


        #region Private Fields                         
        #endregion //Private Fields


        #region Private Functions

        private void ParseLicenseInfosForThanks()
        {       
            System.Collections.Hashtable dataFields = cryptoLicense.ParseUserData("#");
            this.LicenseThanks_HardwareID = cryptoLicense.GetLocalMachineCodeAsString();
            this.LicenseThanks_Organisation = dataFields["Organisation"] as string;
            this.LicenseThanks_Name = dataFields["Name"] as string;
            this.LicenseThanks_Mail = dataFields["Mail"] as string;
            if (dataFields["Liteversion"] as string == "True")
                this.LicenseThanks_Version = "Lite";
            else
                this.LicenseThanks_Version = "Basic";

            this.LicenseThanks_FeatureMailOn = cryptoLicense.IsFeaturePresentEx(1);
            this.LicenseThanks_FeatureMapOn = cryptoLicense.IsFeaturePresentEx(2);
            this.LicenseThanks_FeatureAlarmappOn = cryptoLicense.IsFeaturePresentEx(3);        

            this.LicenseThanks_DateExpires = cryptoLicense.DateExpires.ToString();
            if (!cryptoLicense.HasDateExpires)
                this.LicenseThanks_DateExpires = "Unbegrenzt";

        }
        #endregion //Private Funtions     
    }
}
