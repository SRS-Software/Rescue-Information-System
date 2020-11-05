#region

using System.Windows.Controls;
using RIS.Business;
using RIS.ViewModels;

#endregion

namespace RIS.Views
{
    public partial class SettingsPagerDialog : UserControl
    {
        public SettingsPagerDialog(IBusiness _business, int _id)
        {
            InitializeComponent();

            DataContext = new SettingsPagerViewModel(_business, _id);
        }
    }
}