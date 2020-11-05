#region

using System.Windows;
using System.Windows.Controls;
using RIS.Core.Helper;
using RIS.Properties;

#endregion

namespace RIS.Views
{
    public partial class SettingsDatenschnittstelleUserControl : UserControl
    {
        public SettingsDatenschnittstelleUserControl()
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(Settings.Default.MailOutput_Password))
                uxMailOutput_Password.Password =
                    Encrypt.DecryptString(Settings.Default.MailOutput_Password, "MailOutput_Password");
        }

        private void uxMailOutput_Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Settings.Default.MailOutput_Password =
                Encrypt.EncryptString(((PasswordBox) sender).Password, "MailOutput_Password");
        }
    }
}