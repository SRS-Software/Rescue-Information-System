#region

using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Timers;
using GalaSoft.MvvmLight;
using RIS.Core.Helper;
using RIS.Properties;
using SRS.Utilities;

#endregion

namespace RIS.ViewModels
{
    public class MainTickerViewModel : ViewModelBase
    {
        #region Private Properties

        private Timer tickerTimer;

        #endregion //Private Properties

        public MainTickerViewModel()
        {
            try
            {
                tickerTimer = new Timer
                {
                    Interval = 1000 * 60,
                    AutoReset = false
                };
                tickerTimer.Elapsed += tickerTimer_Elapsed;
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        ~MainTickerViewModel()
        {
            if (tickerTimer != null)
            {
                tickerTimer.Stop();
                tickerTimer = null;
            }
        }

        #region Public Functions

        public void Initialize()
        {
            TickerText = string.Empty;
            tickerTimer_Elapsed(this, null);
        }

        #endregion //Public Functions

        #region Private Functions

        private void tickerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (Settings.Default.Ticker_IsFile)
                {
                    var _fileName = Settings.Default.Ticker_Text;
                    if (WaitFileReady.Check(_fileName) == false)
                    {
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Can not read from textfile");
                        return;
                    }

                    TickerText = File.ReadAllText(_fileName, Encoding.GetEncoding(1252));
                }
                else
                {
                    TickerText = Settings.Default.Ticker_Text;
                }

                //Restart timer
                tickerTimer.Start();
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #endregion //Private Funtions

        #region Commands

        #endregion //Commands

        #region Events

        #endregion //Events

        #region Validation

        #endregion //Validation

        #region Public Properties

        private string tickerText;

        public string TickerText
        {
            get => tickerText;
            set
            {
                if (tickerText == value) return;

                tickerText = value;
                RaisePropertyChanged(() => TickerText);
            }
        }

        #endregion //Public Properties
    }
}