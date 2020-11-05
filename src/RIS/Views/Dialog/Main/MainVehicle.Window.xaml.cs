#region

using System.Windows;
using RIS.ViewModels;

#endregion

namespace RIS.Views
{
    public partial class MainVehicleWindow : Window
    {
        public MainVehicleWindow(int _row, int _column)
        {
            InitializeComponent();

            var _viewModel = new MainVehicleViewModel(_row, _column);
            _viewModel.CloseRequested += (sender, e) =>
            {
                DialogResult = true;
                Close();
            };
            DataContext = _viewModel;
        }
    }
}