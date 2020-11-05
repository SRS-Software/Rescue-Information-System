#region

using System.Windows;
using RIS.ViewModels;

#endregion

namespace RIS.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            var _viewModel = new SettingsViewModel();
            _viewModel.CloseRequested += (sender, e) =>
            {
                DialogResult = true;
                Close();
            };
            DataContext = _viewModel;
        }
    }
}