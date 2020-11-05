#region

using System.Windows.Controls;
using RIS.Business;
using RIS.ViewModels;

#endregion

namespace RIS.Views
{
    public partial class SettingsAlarmappDialog : UserControl
    {
        public SettingsAlarmappDialog(IBusiness _business, int _id)
        {
            InitializeComponent();

            DataContext = new SettingsAlarmappViewModel(_business, _id);
        }
    }
}