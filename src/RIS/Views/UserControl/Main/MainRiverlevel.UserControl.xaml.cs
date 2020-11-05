#region

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using RIS.Properties;
using SRS.Utilities;
using Point = System.Drawing.Point;

#endregion

namespace RIS.Views
{
    public partial class MainRiverlevelUserControl : UserControl
    {
        public MainRiverlevelUserControl()
        {
            InitializeComponent();
        }

        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                Settings.Default.Riverlevel_ImageSize =
                    new Point((int) uxImage.ActualWidth, (int) uxImage.ActualHeight);
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }
    }
}