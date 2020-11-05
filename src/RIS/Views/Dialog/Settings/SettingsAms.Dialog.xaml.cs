#region

using System.Windows.Controls;
using RIS.Business;
using RIS.ViewModels;

#endregion

namespace RIS.Views
{
    public partial class SettingsAmsDialog : UserControl
    {
        public SettingsAmsDialog(IBusiness _business, int _id)
        {
            InitializeComponent();

            DataContext = new SettingsAmsViewModel(_business, _id);
        }
    }
}