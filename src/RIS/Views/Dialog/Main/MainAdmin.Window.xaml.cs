#region

using System.Windows;
using RIS.ViewModels;

#endregion

namespace RIS.Views
{
    public partial class MainAdminWindow : Window
    {
        public MainAdminWindow()
        {
            InitializeComponent();

            var _viewModel = new MainAdminViewModel();
            _viewModel.CloseRequested += (sender, e) =>
            {
                DialogResult = _viewModel.Result;
                Close();
            };
            DataContext = _viewModel;

            uxPasswordBox_LoginPassword.Focus();
        }
    }
}