#region

using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

#endregion

namespace RIS.Views
{
    public partial class AlarmTimerUserControl : UserControl
    {
        public AlarmTimerUserControl()
        {
            InitializeComponent();

            uxClock_SizeChanged(this, null);
        }

        private void uxClock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            uxClock.FontSize = CalculateMaximumFontSize(1000, 8, 4, uxClock.Text, new FontFamily("Segoe UI"),
                new Size(uxClock.ActualWidth, uxClock.ActualHeight));
        }

        private static double CalculateMaximumFontSize(double maximumFontSize, double minimumFontSize,
            double reductionStep, string text, FontFamily fontFamily, Size containerAreaSize)
        {
            var fontSize = maximumFontSize;
            var formattedText = new FormattedText(text, CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight, new Typeface(fontFamily.ToString()), fontSize, Brushes.Black);
            var maximumInternalHeight = containerAreaSize.Height;

            if (formattedText.Height > maximumInternalHeight)
                do
                {
                    fontSize -= reductionStep;
                    formattedText.SetFontSize(fontSize);
                } while (formattedText.Height > maximumInternalHeight && fontSize > minimumFontSize);

            return fontSize;
        }
    }
}