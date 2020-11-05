#region

using System.Windows.Controls;
using RIS.Business;
using RIS.ViewModels;

#endregion

namespace RIS.Views
{
    public partial class SettingsPrinterDialog : UserControl
    {
        public SettingsPrinterDialog(IBusiness _business, int _id)
        {
            InitializeComponent();

            DataContext = new SettingsPrinterViewModel(_business, _id);
        }
    }
}