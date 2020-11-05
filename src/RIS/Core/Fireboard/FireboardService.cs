#region

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using RIS.Business;
using RIS.Core.Decoder;
using RIS.Core.Fax;
using RIS.Properties;
using SRS.Utilities;
using SRS.Utilities.Extensions;
using MessageBox = RIS.Core.Helper.MessageBox;

#endregion

namespace RIS.Core
{
    public class FireboardService : IFireboardService
    {
        private readonly IBusiness _business;
        private readonly IDecoderService _decoderService;
        private readonly IFaxService _faxService;

        #region Private Properties

        private FireboardApiService _apiService;

        #endregion //Private Properties

        public FireboardService(IBusiness business, IDecoderService decoderService, IFaxService faxService)
        {
            try
            {
                _business = business;
                _decoderService = decoderService;
                _faxService = faxService;

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
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                IsRunning = false;
                _apiService = null;

                if (!string.IsNullOrEmpty(Settings.Default.Fireboard_AuthKey))
                {
                    _apiService = new FireboardApiService(Settings.Default.Fireboard_AuthKey);
                    if (!_apiService.IsAuthTokenValid())
                    {
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), "AuthKey -> invalid");
                        MessageBox.Show(
                            "Es ist ein Fehler bei der Verbindung zur Fireboard-API aufgetreten, bitte Überprüfen Sie Ihre Zugangsdaten!",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), "AuthKey -> valid");
                        registerEvents();
                    }
                }

                IsRunning = true;

                stopWatch.Stop();
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"Started -> {stopWatch.Elapsed}");
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
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                unregisterEvents();

                stopWatch.Stop();
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"Stopped -> {stopWatch.Elapsed}");
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

        #region Private Funtions

        private void registerEvents()
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Register Events");

            _decoderService.FmsMessageReceived += decoderService_FmsMessageReceived;
            _decoderService.PagerMessageReceived += decoderService_PagerMessageReceived;
            _faxService.EinsatzCreated += faxService_EinsatzCreated;
        }

        private void unregisterEvents()
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Unregister Events");

            _decoderService.FmsMessageReceived -= decoderService_FmsMessageReceived;
            _decoderService.PagerMessageReceived -= decoderService_PagerMessageReceived;
            _faxService.EinsatzCreated -= faxService_EinsatzCreated;
        }

        private void decoderService_FmsMessageReceived(object sender, FmsMessageEventArgs e)
        {
            if (e == null || e.Vehicle == null || e.Status != "C") return;

            if (_apiService == null) return;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"FMS-Message({e.Identifier}->{e.Status})");
                }
                catch (Exception ex)
                {
                    ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs
                    {
                        Methode = MethodBase.GetCurrentMethod(),
                        Error = ex
                    });
                }
            });
        }

        private void decoderService_PagerMessageReceived(object sender, PagerMessageEventArgs e)
        {
            if (e == null || e.Pager == null) return;

            if (_apiService == null) return;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"Pager-Message({e.Identifier})");
                }
                catch (Exception ex)
                {
                    ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs
                    {
                        Methode = MethodBase.GetCurrentMethod(),
                        Error = ex
                    });
                }
            });
        }

        private void faxService_EinsatzCreated(object sender, EinsatzCreatedEventArgs e)
        {
            if (e == null || e.Einsatz == null) return;

            if (_apiService == null) return;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Fax-Message");

                    _apiService.CreateAlarm(e.Einsatz);
                }
                catch (Exception ex)
                {
                    ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs
                    {
                        Methode = MethodBase.GetCurrentMethod(),
                        Error = ex
                    });
                }
            });
        }

        #endregion //Private Funtions
    }
}