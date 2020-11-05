#region

using System;
using System.Windows.Controls;
using RIS.Properties;

#endregion

namespace RIS.Views
{
    public partial class AlarmDataUserControl : UserControl
    {
        public AlarmDataUserControl()
        {
            InitializeComponent();
            Unloaded += AlarmDataUserControl_Unloaded;
        }

        public void AlarmDataUserControl_Unloaded(object sender, EventArgs e)
        {
            Settings.Default.AlarmData_Width = ActualWidth;
            Settings.Default.Save();
        }
    }
}