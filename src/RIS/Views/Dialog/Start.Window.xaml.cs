#region

using System.Reflection;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using SRS.Utilities;
using Timer = System.Timers.Timer;

#endregion

namespace RIS.Views
{
    partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
        }
    }

    public class StartSplashScreen
    {
        private StartWindow startWindow { get; set; }

        public void Show()
        {
            Timer _timeoutTimer = null;

            Close();

            var thread = new Thread(() =>
            {
                startWindow = new StartWindow();
                startWindow.Closed += (sender2, e2) =>
                {
                    startWindow.Dispatcher.InvokeShutdown();
                    startWindow = null;

                    if (_timeoutTimer != null)
                    {
                        _timeoutTimer.Stop();
                        _timeoutTimer = null;
                    }
                };

                startWindow.Show();

                Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            _timeoutTimer = new Timer();
            _timeoutTimer.Interval = 60000;
            _timeoutTimer.Elapsed += TimeoutTimer_Elapsed;
            _timeoutTimer.Start();

            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "StartSplashScreen -> Show");
        }

        public void Close()
        {
            try
            {
                if (startWindow == null) return;

                startWindow.Dispatcher.Invoke(() => startWindow.Close());

                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "StartSplashScreen -> Close");
            }
            catch
            {
            }
        }

        private void TimeoutTimer_Elapsed(object source, ElapsedEventArgs e)
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "StartSplashScreen -> Timeout");
            Close();
        }
    }
}