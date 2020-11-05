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
    public partial class MainWarnweatherUserControl : UserControl
    {
        public MainWarnweatherUserControl()
        {
            InitializeComponent();
        }

        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                Settings.Default.Warnweather_ImageSize =
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