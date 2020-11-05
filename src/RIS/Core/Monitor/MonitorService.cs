#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Timers;
using GalaSoft.MvvmLight.Threading;
using RIS.Core.Decoder;
using RIS.Core.Helper;
using RIS.Properties;
using RIS.Views;
using SRS.Utilities;
using SRS.Utilities.Extensions;

#endregion

namespace RIS.Core
{
    public class MonitorService : IMonitorService
    {
        private readonly IDecoderService _decoderService;
        private readonly IMouseService _mouseService;

        public MonitorService(IDecoderService decoderService, IMouseService mouseService)
        {
            try
            {
                _decoderService = decoderService;
                _mouseService = mouseService;

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
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Starting");
                var _stopWatch = new Stopwatch();
                _stopWatch.Start();

                IsRunning = false;

                //Load monitor weekplan    
                _monitorSettings = Serializer.Deserialize<SettingsMonitor>(Settings.Default.Monitor_Weekplan);
                _alarmWindows = new List<AlarmWindow>();
                _timeFmsElapsed = null;
                _timePagerElapsed = null;
                _timeWindowElapsed = null;
                _timeMouseElapsed = DateTime.Now.Add(new TimeSpan(0, 1, 0));

                //Initialize Screen and Screensaver
                Screen.SwitchOn();
                Screensaver.Enable();

                //Timer to check 
                _monitorTimer = new Timer
                {
                    Interval = 1000,
                    AutoReset = false,
                    Enabled = true
                };
                _monitorTimer.Elapsed += monitorTimer_Elapsed;
                _monitorTimer.Start();

                registerEvents();
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

                unregisterEvents();

                //Close all open alarmWindows
                foreach (var _alarmWindow in _alarmWindows)
                    DispatcherHelper.CheckBeginInvokeOnUI(() => { _alarmWindow.Close(); });

                _alarmWindows.Clear();

                //Disable timer for Monitor on/off
                if (_monitorTimer != null)
                {
                    _monitorTimer.Stop();
                    _monitorTimer = null;
                }

                //Switch Monitor on,
                Screen.SwitchOn();
                Screensaver.Enable();

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

        public void AddAlarmWindow(AlarmWindow _alarmWindow)
        {
            try
            {
                _alarmWindows.Add(_alarmWindow);
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

        public void RemoveAlarmWindow(AlarmWindow _alarmWindow)
        {
            try
            {
                _alarmWindows.Remove(_alarmWindow);

                //Check if all alarmWindows closed to add delay time 
                if (_alarmWindows.Count == 0 && Settings.Default.Monitor_AlarmDelayTime.TotalSeconds > 0)
                    _timeWindowElapsed = DateTime.Now.Add(Settings.Default.Monitor_AlarmDelayTime);
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

        public bool IsAlarmWindow()
        {
            try
            {
                if (_alarmWindows == null)
                    return false;

                return _alarmWindows.Count > 0;
            }
            catch (Exception ex)
            {
                ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs
                {
                    Methode = MethodBase.GetCurrentMethod(),
                    Error = ex
                });
                return false;
            }
        }

        #endregion //Public Funtions

        #region Private Properties

        private SettingsMonitor _monitorSettings;
        private Timer _monitorTimer;

        private DateTime? _timeFmsElapsed;
        private DateTime? _timePagerElapsed;
        private DateTime? _timeWindowElapsed;
        private DateTime? _timeMouseElapsed;

        private List<AlarmWindow> _alarmWindows = new List<AlarmWindow>();

        #endregion //Private Properties

        #region Private Funtions

        private void registerEvents()
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Register Events");

            _decoderService.FmsMessageReceived += decoderService_FmsMessageReceived;
            _decoderService.PagerMessageReceived += decoderService_PagerMessageReceived;
            _mouseService.MouseMoved += mouseService_MouseMoved;
        }

        private void unregisterEvents()
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Unregister Events");

            _decoderService.FmsMessageReceived -= decoderService_FmsMessageReceived;
            _decoderService.PagerMessageReceived -= decoderService_PagerMessageReceived;
            _mouseService.MouseMoved -= mouseService_MouseMoved;
        }

        private void decoderService_FmsMessageReceived(object sender, FmsMessageEventArgs e)
        {
            if (e == null || e.Vehicle == null || e.Vehicle.MainColumn == null || e.Vehicle.MainRow == null) return;

            if (Settings.Default.Monitor_WakeupStatusTime.TotalSeconds > 0)
                _timeFmsElapsed = DateTime.Now.Add(Settings.Default.Monitor_WakeupStatusTime);
        }

        private void decoderService_PagerMessageReceived(object sender, PagerMessageEventArgs e)
        {
            if (e == null || e.Pager == null) return;

            if (Settings.Default.Monitor_WakeupAlarmTime.TotalSeconds > 0)
                _timePagerElapsed = DateTime.Now.Add(Settings.Default.Monitor_WakeupAlarmTime);
        }

        private void mouseService_MouseMoved(object sender, EventArgs e)
        {
            _timeMouseElapsed = DateTime.Now.Add(new TimeSpan(0, 1, 0));
        }

        private void monitorTimer_Elapsed(object source, ElapsedEventArgs e)
        {
            //Check if it is time to close a alarmWindow  
            foreach (var _alarmWindow in _alarmWindows)
                if (DateTime.Now > _alarmWindow.CloseTime)
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "AlarmWindowTimer -> Elapsed");

                    DispatcherHelper.CheckBeginInvokeOnUI(() => { _alarmWindow.Close(); });
                }

            //Check if a time is elapsed
            if (_timeFmsElapsed != null && DateTime.Now > _timeFmsElapsed)
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "FmsTimer -> Elapsed");
                _timeFmsElapsed = null;
            }

            if (_timePagerElapsed != null && DateTime.Now > _timePagerElapsed)
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "PagerTimer -> Elapsed");
                _timePagerElapsed = null;
            }

            if (_timeWindowElapsed != null && DateTime.Now > _timeWindowElapsed)
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "WindowTimer -> Elapsed");
                _timeWindowElapsed = null;
            }

            if (_timeMouseElapsed != null && DateTime.Now > _timeMouseElapsed)
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "MouseTimer -> Elapsed");
                _timeMouseElapsed = null;
            }

            //Load current day list
            ObservableCollection<SettingsMonitor.MonitorItem> _dayList = null;
            switch (DateTime.Now.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    _dayList = _monitorSettings.Monday;
                    break;
                case DayOfWeek.Tuesday:
                    _dayList = _monitorSettings.Tuesday;
                    break;
                case DayOfWeek.Wednesday:
                    _dayList = _monitorSettings.Wednesday;
                    break;
                case DayOfWeek.Thursday:
                    _dayList = _monitorSettings.Thursday;
                    break;
                case DayOfWeek.Friday:
                    _dayList = _monitorSettings.Friday;
                    break;
                case DayOfWeek.Saturday:
                    _dayList = _monitorSettings.Saturday;
                    break;
                case DayOfWeek.Sunday:
                    _dayList = _monitorSettings.Sunday;
                    break;
            }

            //Check to switch monitor on/off
            if (IsAlarmWindow())
            {
                MonitorOn();
                ScreensaverOff();
            }
            else if (_timeFmsElapsed != null || _timePagerElapsed != null || _timeWindowElapsed != null ||
                     _timeMouseElapsed != null)
            {
                MonitorOn();
                ScreensaverOff();
            }
            else if (Settings.Default.Monitor_WeekplanDisabled || _dayList == null)
            {
                MonitorOn();
                ScreensaverOn();
            }
            else if (_dayList
                .Where(d => DateTime.Now.TimeOfDay >= d.Start.TimeOfDay && DateTime.Now.TimeOfDay < d.Stop.TimeOfDay)
                .FirstOrDefault() != null)
            {
                MonitorOn();
                ScreensaverOff();
            }
            else
            {
                MonitorOff();
                ScreensaverOn();
            }

            //Start timer again
            _monitorTimer.Start();
        }

        private void MonitorOn()
        {
            if (Screen.Status == false)
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Monitor -> ON");

                Screen.SwitchOn();
            }
        }

        private void MonitorOff()
        {
            if (Screen.Status)
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Monitor -> OFF");

                Screen.SwitchOff();
            }
        }

        private void ScreensaverOn()
        {
            if (Screensaver.Status == false)
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Screensaver -> ON");

                Screensaver.Enable();
            }
        }

        private void ScreensaverOff()
        {
            if (Screensaver.Status)
            {
                if (Screensaver.IsRunning())
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Screensaver -> KILL");
                    Screensaver.Kill();
                }

                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Screensaver -> OFF");
                Screensaver.Disable();
            }
        }

        #endregion //Private Funtions
    }
}