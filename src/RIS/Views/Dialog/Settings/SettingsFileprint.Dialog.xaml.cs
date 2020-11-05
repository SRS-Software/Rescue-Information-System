#region

using System.Windows.Controls;
using RIS.Business;
using RIS.ViewModels;

#endregion

namespace RIS.Views
{
    public partial class SettingsFileprintDialog : UserControl
    {
        public SettingsFileprintDialog(IBusiness _business, int _id)
        {
            InitializeComponent();

            DataContext = new SettingsFileprintViewModel(_business, _id);
        }
    }
}