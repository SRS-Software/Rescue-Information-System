#region

using System;
using System.Globalization;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GalaSoft.MvvmLight.Threading;

#endregion

namespace RIS.Views
{
    public partial class MainTimeUserControl : UserControl
    {
        public MainTimeUserControl()
        {
            InitializeComponent();

            uxClock_SizeChanged(this, null);

            updateTimer = new Timer();
            updateTimer.Interval = 1000;
            updateTimer.Elapsed += (sender, e) => DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                updateTimer_Elapsed(sender, e);
                updateTimer.Start();
            });
            updateTimer.Start();
        }

        private Timer updateTimer { get; }

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
            var maximumpublicHeight = containerAreaSize.Height;

            if (formattedText.Height > maximumpublicHeight)
                do
                {
                    fontSize -= reductionStep;
                    formattedText.SetFontSize(fontSize);
                } while (formattedText.Height > maximumpublicHeight && fontSize > minimumFontSize);

            return fontSize;
        }

        private void updateTimer_Elapsed(object source, ElapsedEventArgs e)
        {
            uxClock.Text = DateTime.Now.ToString("dd.MM.yyyy   HH:mm:ss");
        }
    }
}