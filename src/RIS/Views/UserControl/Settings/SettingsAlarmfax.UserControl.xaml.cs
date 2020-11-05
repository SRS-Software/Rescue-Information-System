#region

using System.Windows;
using System.Windows.Controls;
using RIS.Core.Helper;
using RIS.Properties;

#endregion

namespace RIS.Views
{
    public partial class SettingsAlarmfaxUserControl : UserControl
    {
        public SettingsAlarmfaxUserControl()
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(Settings.Default.MailInput_Password))
                uxMailInput_Password.Password =
                    Encrypt.DecryptString(Settings.Default.MailInput_Password, "MailInput_Password");
        }

        private void uxMailInput_Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Settings.Default.MailInput_Password =
                Encrypt.EncryptString(((PasswordBox) sender).Password, "MailInput_Password");
        }
    }
}