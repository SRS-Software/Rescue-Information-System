#region

using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

#endregion

namespace RIS.Views
{
    public partial class VehicleUserControl : UserControl
    {
        public VehicleUserControl()
        {
            InitializeComponent();
        }
    }

    public class StatusWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double) value / 3;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double) value * 3;
        }
    }
}