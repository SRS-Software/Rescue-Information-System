#region

using System.Windows.Controls;
using RIS.Business;
using RIS.ViewModels;

#endregion

namespace RIS.Views
{
    public partial class SettingsFilterDialog : UserControl
    {
        public SettingsFilterDialog(IBusiness _business, int _id)
        {
            InitializeComponent();

            DataContext = new SettingsFilterViewModel(_business, _id);
        }
    }
}