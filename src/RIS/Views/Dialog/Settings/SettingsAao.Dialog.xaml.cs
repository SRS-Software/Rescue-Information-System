#region

using System.Windows.Controls;
using RIS.Business;
using RIS.ViewModels;

#endregion

namespace RIS.Views
{
    public partial class SettingsAaoDialog : UserControl
    {
        public SettingsAaoDialog(IBusiness _business, int _id)
        {
            InitializeComponent();

            DataContext = new SettingsAaoViewModel(_business, _id);
        }
    }
}