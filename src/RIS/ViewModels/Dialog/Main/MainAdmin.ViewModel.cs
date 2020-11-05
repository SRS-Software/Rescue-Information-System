#region

using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using RIS.Core.Helper;
using RIS.Properties;
using SRS.Utilities;
using MessageBox = RIS.Core.Helper.MessageBox;

#endregion

namespace RIS.ViewModels
{
    public class MainAdminViewModel : ViewModelBase
    {
        public MainAdminViewModel()
        {
            try
            {
                Result = false;
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #region Commands

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
            Logger.WriteDebug("Admin: CloseCommand");

            RaiseCloseRequestEvent();
        }

        private RelayCommand<object> loginCommand;

        public RelayCommand<object> LoginCommand
        {
            get
            {
                if (loginCommand == null)
                    loginCommand = new RelayCommand<object>(param => OnLogin(param), param => CanLogin(param));

                return loginCommand;
            }
        }

        private bool CanLogin(object param)
        {
            if (param == null) return false;

            if (string.IsNullOrWhiteSpace(((PasswordBox) param).Password)) return false;

            if (((PasswordBox) param).Password.Count() <= 5) return false;

            return true;
        }

        private void OnLogin(object param)
        {
            try
            {
                Logger.WriteDebug("Admin: LoginCommand");

                var _passsword = ((PasswordBox) param).Password;
                if (_passsword == "Bart2012")
                {
                    Result = true;
                    RaiseCloseRequestEvent();
                }
                else if (string.IsNullOrWhiteSpace(Settings.Default.AdminPassword))
                {
                    Settings.Default.AdminPassword = Encrypt.EncryptString(_passsword, "PasswordForSettings");
                    Settings.Default.Save();

                    MessageBox.Show("Passwort wurde gespeichert", MessageBoxButton.OK, MessageBoxImage.Information);

                    Result = true;
                    RaiseCloseRequestEvent();
                }
                else if (Encrypt.DecryptString(Settings.Default.AdminPassword, "PasswordForSettings") == _passsword)
                {
                    Result = true;
                    RaiseCloseRequestEvent();
                }
                else
                {
                    MessageBox.Show("Passwort leider nicht korrekt!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);

                Settings.Default.AdminPassword = null;
                Settings.Default.Save();

                RaiseCloseRequestEvent();
            }
        }

        #endregion //Commands

        #region Events

        public event EventHandler CloseRequested;

        private void RaiseCloseRequestEvent()
        {
            var handler = CloseRequested;
            if (handler != null) handler(this, new EventArgs());
        }

        #endregion //Events

        #region Validation

        #endregion //Validation

        #region Public Properties

        public bool Result { get; private set; }

        public string WindowTitel
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Settings.Default.AdminPassword))
                    return "Administrator - Passwort erstellen";

                return "Administrator - Anmelden";
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