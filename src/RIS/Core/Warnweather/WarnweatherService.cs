#region

using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Timers;
using RIS.Properties;
using SRS.Utilities;
using SRS.Utilities.Extensions;

#endregion

namespace RIS.Core
{
    public class WarnweatherService : IWarnweatherService
    {
        private readonly Timer _updateTimer;

        public WarnweatherService()
        {
            try
            {
                _updateTimer = new Timer();
                _updateTimer.AutoReset = false;
                _updateTimer.Interval = 60000 * 10; //10min
                _updateTimer.Elapsed += updateTimer_Elapsed;

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

        #region Public Funtions

        public void Start()
        {
            try
            {
                if (string.IsNullOrEmpty(Settings.Default.Warnweather_Url))
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Warnweather disabled in settings");
                    return;
                }

                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Starting");
                var _stopWatch = new Stopwatch();
                _stopWatch.Start();

                IsRunning = false;

                //Query first time
                updateTimer_Elapsed(this, null);

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

                _updateTimer.Stop();

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
        public event EventHandler<byte[]> ImageReceived;

        #endregion //Events

        #region Private Properties

        #endregion //Private Properties

        #region Private Funtions

        private void updateTimer_Elapsed(object source, ElapsedEventArgs e)
        {
            try
            {
                var _imageUrl = Settings.Default.Warnweather_Url;
                if (!_imageUrl.Contains("WIDTH") && !_imageUrl.Contains("HEIGHT"))
                {
                    _imageUrl += $"&WIDTH={Settings.Default.Warnweather_ImageSize.X}";
                    _imageUrl += $"&HEIGHT={Settings.Default.Warnweather_ImageSize.Y}";
                }

                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                                       SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                using (var _client = new WebClient())
                {
                    _client.DownloadDataCompleted += client_DownloadImageCompleted;
                    _client.DownloadDataAsync(new Uri(_imageUrl));
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
            finally
            {
                //Restart timer
                _updateTimer.Start();
            }
        }

        private void client_DownloadImageCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    Logger.WriteError(MethodBase.GetCurrentMethod(), e.Error.Message);
                    return;
                }

                if (e.Result != null) ImageReceived.Invoke(this, e.Result);
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