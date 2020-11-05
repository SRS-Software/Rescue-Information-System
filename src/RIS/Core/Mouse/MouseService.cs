#region

using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Timers;
using System.Windows.Forms;
using SRS.Utilities;
using SRS.Utilities.Extensions;
using Timer = System.Timers.Timer;

#endregion

namespace RIS.Core
{
    public class MouseService : IMouseService
    {
        private readonly Timer _refreshTimer;

        #region Private Properties

        private Point _oldPosition;

        #endregion //Private Properties

        public MouseService()
        {
            try
            {
                _refreshTimer = new Timer
                {
                    Interval = 100,
                    AutoReset = false
                };
                _refreshTimer.Elapsed += refreshTimer_Elapsed;

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


        #region Private Funtions

        private void refreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                //Position changed (0,0 set if screensaver activated)
                if (_oldPosition != Cursor.Position && Cursor.Position != new Point(0, 0))
                {
                    _oldPosition = Cursor.Position;
                    MouseMoved.RaiseEvent(this, new EventArgs());
                }

                //Restart timer        
                _refreshTimer.Start();
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

        #region Public Funtions

        public void Start()
        {
            try
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Starting");
                var _stopWatch = new Stopwatch();
                _stopWatch.Start();

                IsRunning = false;
                _oldPosition = Cursor.Position;
                _refreshTimer.Start();
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

                _refreshTimer.Stop();

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

        #region Events

        public event EventHandler<ExceptionEventArgs> ExceptionOccured;
        public event EventHandler MouseMoved;

        #endregion //Events
    }
}