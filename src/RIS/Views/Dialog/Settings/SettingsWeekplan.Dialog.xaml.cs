#region

using System.Windows;
using RIS.ViewModels;

#endregion

namespace RIS.Views
{
    public partial class SettingsWeekplanDialog : Window
    {
        public SettingsWeekplanDialog()
        {
            InitializeComponent();

            var _viewModel = new SettingsWeekplanViewModel();
            _viewModel.CloseRequested += (sender, e) =>
            {
                DialogResult = _viewModel.Result;
                Close();
            };
            DataContext = _viewModel;
        }
    }
}