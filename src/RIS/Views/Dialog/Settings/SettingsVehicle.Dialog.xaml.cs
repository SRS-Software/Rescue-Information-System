#region

using System.Windows.Controls;
using RIS.Business;
using RIS.ViewModels;

#endregion

namespace RIS.Views
{
    public partial class SettingsVehicleDialog : UserControl
    {
        public SettingsVehicleDialog(IBusiness _business, int _id)
        {
            InitializeComponent();

            DataContext = new SettingsVehicleViewModel(_business, _id);
        }
    }
}