#region

using System;
using System.Drawing;
using System.Windows.Controls;
using RIS.Properties;

#endregion

namespace RIS.Views
{
    public partial class AlarmRouteUserControl : UserControl
    {
        public AlarmRouteUserControl()
        {
            InitializeComponent();

            Unloaded += AlarmRouteUserControl_Unloaded;
        }

        public void AlarmRouteUserControl_Unloaded(object sender, EventArgs e)
        {
            Settings.Default.Route_ImageSize = new Point((int) ActualWidth, (int) ActualHeight);
            Settings.Default.Save();
        }
    }
}