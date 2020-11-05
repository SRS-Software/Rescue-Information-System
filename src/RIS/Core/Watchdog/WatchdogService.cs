#region

using System;
using System.Diagnostics;
using System.Reflection;
using System.Timers;
using SRS.Utilities;
using SRS.Utilities.Extensions;

#endregion

namespace RIS.Core
{
    public class WatchdogService : IWatchdogService
    {
        private readonly Timer _refreshTimer;

        public WatchdogService()
        {
            try
            {
                _refreshTimer = new Timer
                {
                    Interval = 60000 * 5,
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

        #region Events

        public event EventHandler<ExceptionEventArgs> ExceptionOccured;

        #endregion //Events

        #region Private Funtions

        private void refreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Task.Factory.StartNew(() =>
            //{
            //    try
            //    {
            //        Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Watchdog -> ping");

            //        //Query license from Webservice
            //        RestRequest request = new RestRequest(@"api/organisations/{organisationId}/watchdog", Method.POST);
            //        request.AddUrlSegment("organisationId", RIS.Properties.Settings.Default.Webservice_OrganisationId);
            //        request.AddParameter("hardwareId", _licenseService.HardwareId);
            //        request.AddParameter("hardwareDescription", System.Environment.MachineName);
            //        var response = _webService.ApiService.Execute<WatchdogResponse>(request);
            //        if (response?.Result == "Error")
            //        {
            //            Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"WatchdogResponse -> {response?.Message}");
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs()
            //        {
            //            Methode = MethodBase.GetCurrentMethod(),
            //            Error = ex,
            //        });
            //    }
            //}).ContinueWith((prevTask) =>
            //{
            //    //Restart timer        
            //    _refreshTimer.Start();
            //});
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
                refreshTimer_Elapsed(this, null);
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

        #region Private Properties

        #endregion //Private Properties
    }
}