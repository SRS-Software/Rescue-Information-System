#region

using System;
using System.Diagnostics;
using System.Reflection;
using System.Timers;
using System.Windows;
using GalaSoft.MvvmLight.Threading;
using RIS.Properties;
using SRS.Utilities;
using SRS.Utilities.Extensions;

#endregion

namespace RIS.Core
{
    public class RebootService : IRebootService
    {
        private readonly IMonitorService _monitorService;
        private readonly Timer _rebootTimer;

        public RebootService(IMonitorService monitorService)
        {
            try
            {
                _monitorService = monitorService;

                _rebootTimer = new Timer();
                _rebootTimer.AutoReset = false;
                _rebootTimer.Elapsed += (sender, e) => DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    rebootTimer_Elapsed(sender, e);
                });

                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Initialize");
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #region Public Properties

        public bool IsRunning { get; private set; }

        #endregion //Public Properties

        #region Events

        public event EventHandler<ExceptionEventArgs> ExceptionOccured;

        #endregion //Events

        #region Public Funtions

        public void Start()
        {
            try
            {
                if (Settings.Default.Reboot_On == false)
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Reboot disabled in settings");
                    return;
                }

                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Starting");
                var _stopWatch = new Stopwatch();
                _stopWatch.Start();

                IsRunning = false;

                _rebootTimer.Interval = MsUntilMidnight();
                _rebootTimer.Start();
                Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                    string.Format("Time until reboot: " +
                                  TimeSpan.FromMilliseconds(_rebootTimer.Interval).ToString(@"hh\:mm\:ss", null)));

                IsRunning = true;

                _stopWatch.Stop();
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"Started -> {_stopWatch.Elapsed}");
            }
            catch (Exception ex)
            {
                ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs
                {
                    Methode = MethodBase.GetCurrentMethod(),
                    Error = ex
                });
            }
        }

        public void Stop()
        {
            try
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Stopping");
                var _stopWatch = new Stopwatch();
                _stopWatch.Start();

                _rebootTimer?.Stop();

                _stopWatch.Stop();
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"Stopped -> {_stopWatch.Elapsed}");
            }
            catch (Exception ex)
            {
                ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs
                {
                    Methode = MethodBase.GetCurrentMethod(),
                    Error = ex
                });
            }
            finally
            {
                IsRunning = false;
            }
        }

        #endregion //Public Funtions

        #region Private Properties

        #endregion //Private Properties

        #region Private Funtions

        private void rebootTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                //Try to restart
                if (_monitorService.IsAlarmWindow() == false)
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Try to reboot system");

                    RebootSystem();
                }
                else
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Reboot not possible -> alarmwindow open");

                    //Restart timer    
                    _rebootTimer.Interval = MsUntilMidnight();
                    _rebootTimer.Start();
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                        string.Format("Time until reboot: " + TimeSpan.FromMilliseconds(_rebootTimer.Interval)
                            .ToString(@"hh\:mm\:ss", null)));
                }
            }
            catch (Exception ex)
            {
                ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs
                {
                    Methode = MethodBase.GetCurrentMethod(),
                    Error = ex
                });
            }
        }

        private int MsUntilMidnight()
        {
            var ts = DateTime.Today.AddDays(1).Subtract(DateTime.Now);
            return (int) ts.TotalMilliseconds;
        }

        private void RebootSystem()
        {
            try
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Shutdown!");

                Process.Start("shutdown.exe", "/r /f /t 5");

                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs
                {
                    Methode = MethodBase.GetCurrentMethod(),
                    Error = ex
                });
            }
        }

        #endregion //Private Funtions
    }
}