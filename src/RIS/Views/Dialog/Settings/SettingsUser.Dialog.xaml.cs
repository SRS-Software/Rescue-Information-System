#region

using System.Windows.Controls;
using RIS.Business;
using RIS.ViewModels;

#endregion

namespace RIS.Views
{
    public partial class SettingsUserDialog : UserControl
    {
        public SettingsUserDialog(IBusiness _business, int _id)
        {
            InitializeComponent();

            DataContext = new SettingsUserViewModel(_business, _id);
        }
    }
}