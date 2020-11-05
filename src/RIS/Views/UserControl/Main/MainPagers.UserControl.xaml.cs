#region

using System;
using System.Windows.Controls;
using System.Windows.Threading;

#endregion

namespace RIS.Views
{
    public partial class MainPagersUserControl : UserControl
    {
        public MainPagersUserControl()
        {
            InitializeComponent();

            uxDataGrid_Pagers.SelectionChanged += uxDataGrid_Pagers_SelectionChanged;
        }

        private void uxDataGrid_Pagers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid)
            {
                var _dataGrid = sender as DataGrid;
                Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => { _dataGrid.UnselectAll(); }));
            }
        }
    }
}