#region

using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using RIS.Properties;
using SRS.Utilities;

#endregion

namespace RIS.Views
{
    public partial class MainTickerUserControl : UserControl
    {
        public MainTickerUserControl()
        {
            InitializeComponent();

            uxStack.SizeChanged += (sender, e) => { InitializeTicker(); };
            uxText.SizeChanged += (sender, e) => { InitializeTicker(); };
            var _dpd =
                DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock));
            _dpd.AddValueChanged(uxText, (a, b) => { InitializeTicker(); });
        }

        private void InitializeTicker()
        {
            try
            {
                //Calculate maximum FontSize                                       
                double _fontSize = 5;
                var _formattedText = new FormattedText(uxText.Text, CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, new Typeface(uxText.FontFamily.ToString()), _fontSize, Brushes.Black);

                do
                {
                    _fontSize++;
                    _formattedText.SetFontSize(_fontSize);
                } while (_formattedText.Height < uxStack.ActualHeight && _fontSize < 35791);

                uxText.FontSize = _fontSize;

                //Calculate Animation
                var _doubleAnimation = new DoubleAnimation
                {
                    From = uxStack.ActualWidth,
                    To = -uxText.ActualWidth,
                    Duration = new Duration(new TimeSpan(0, 0, 0,
                        (int) (uxStack.ActualWidth + uxText.ActualWidth) / Settings.Default.Ticker_Speed)),
                    RepeatBehavior = RepeatBehavior.Forever
                };

                var _translateTransform = new TranslateTransform();
                uxStack.RenderTransform = _translateTransform;
                _translateTransform.BeginAnimation(TranslateTransform.XProperty, _doubleAnimation);
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }
    }
}