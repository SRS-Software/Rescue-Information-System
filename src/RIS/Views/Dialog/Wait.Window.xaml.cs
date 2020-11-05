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
    partial class WaitWindow : Window
    {
        public WaitWindow()
        {
            InitializeComponent();
        }
    }

    public class WaitSplashScreen
    {
        private WaitWindow waitWindow { get; set; }

        public void Show()
        {
            Timer _timeoutTimer = null;

            Close();

            var thread = new Thread(() =>
            {
                waitWindow = new WaitWindow();
                waitWindow.Closed += (sender2, e2) =>
                {
                    waitWindow.Dispatcher.InvokeShutdown();
                    waitWindow = null;

                    if (_timeoutTimer != null)
                    {
                        _timeoutTimer.Stop();
                        _timeoutTimer = null;
                    }
                };

                waitWindow.Show();

                Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            _timeoutTimer = new Timer();
            _timeoutTimer.Interval = 60000;
            _timeoutTimer.Elapsed += TimeoutTimer_Elapsed;
            _timeoutTimer.Start();
        }

        public void Close()
        {
            if (waitWindow == null) return;

            waitWindow.Dispatcher.Invoke(() => waitWindow.Close());
        }

        private void TimeoutTimer_Elapsed(object source, ElapsedEventArgs e)
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "WaitSplashScreen -> Timeout");
            Close();
        }
    }
}