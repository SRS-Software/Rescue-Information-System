#region

using System;
using System.Windows.Controls;
using System.Windows.Input;
using RIS.ViewModels;

#endregion

namespace RIS.Views
{
    public partial class MainVehiclesUserControl : UserControl
    {
        private bool isContextMenuOpen;

        public MainVehiclesUserControl()
        {
            InitializeComponent();
        }

        private void uxGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            isContextMenuOpen = true;
        }

        private void uxGrid_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            isContextMenuOpen = false;
        }

        private void uxGrid_MouseMove(object sender, MouseEventArgs e)
        {
            var _viewModel = DataContext as MainVehiclesViewModel;
            if (_viewModel == null || isContextMenuOpen) return;

            var grid = sender as Grid;
            if (grid == null || grid.ColumnDefinitions == null || grid.ColumnDefinitions.Count <= 0 ||
                grid.RowDefinitions == null || grid.RowDefinitions.Count <= 0)
                return;

            var mousePoint = e.GetPosition(grid);
            if (mousePoint == null) return;

            _viewModel.MouseColumn =
                Convert.ToInt32(Math.Truncate(mousePoint.X / grid.ColumnDefinitions[0].ActualWidth));
            _viewModel.MouseRow = Convert.ToInt32(Math.Truncate(mousePoint.Y / grid.RowDefinitions[0].ActualHeight));
            if (_viewModel.MouseColumn > grid.ColumnDefinitions.Count ||
                _viewModel.MouseRow > grid.RowDefinitions.Count)
            {
                _viewModel.MouseColumn = null;
                _viewModel.MouseRow = null;
            }
        }
    }
}