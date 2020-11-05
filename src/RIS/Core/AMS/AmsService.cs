#region

using System;
using System.Diagnostics;
using System.Reflection;
using System.Timers;
using NAudio.Lame;
using NAudio.Wave;
using RIS.Business;
using RIS.Core.Ams;
using RIS.Core.Decoder;
using SRS.Utilities;
using SRS.Utilities.Extensions;

#endregion

namespace RIS.Core
{
    public class AmsService : IAmsService
    {
        private readonly IBusiness _business;
        private readonly IDecoderService _decoderService;
        private readonly Timer _timerAlarmEnd;
        private readonly Timer _timerRecordEnd;

        private readonly Timer _timerStatusEnd;

        public AmsService(IBusiness business, IDecoderService decoderService)
        {
            try
            {
                _business = business;
                _decoderService = decoderService;

                //Zeit von Status bis Alarmierung sonst löschen
                _timerStatusEnd = new Timer
                {
                    Interval = 10000,
                    AutoReset = false
                };
                _timerStatusEnd.Elapsed += timerStatusEnd_Elapsed;

                //Zeit keine Alarm(Pager) -> Alarmende
                _timerAlarmEnd = new Timer
                {
                    Interval = 25000,
                    AutoReset = false
                };
                _timerAlarmEnd.Elapsed += timerAlarmEnd_Elapsed;

                //Zeit nach Alarmende weiter aufnehmen
                _timerRecordEnd = new Timer
                {
                    Interval = 30000,
                    AutoReset = false
                };
                _timerRecordEnd.Elapsed += timerRecordEnd_Elapsed;

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
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Starting");
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                IsRunning = false;

                if (_business.GetAmsOverview().Count == 0)
                {
                    Logger.WriteError(MethodBase.GetCurrentMethod(), "No AMS configurated");
                    return;
                }

                registerEvents();

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

                stopRecording();
                _einsatz = null;

                //Stop all running timer without elapsed event
                _timerStatusEnd?.Stop();
                _timerAlarmEnd?.Stop();
                _timerRecordEnd?.Stop();

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

        #region Events

        public event EventHandler<ExceptionEventArgs> ExceptionOccured;
        public event EventHandler<EinsatzFinishedEventArgs> EinsatzFinished;

        #endregion //Events

        #region Private Properties

        private WaveInEvent _recordWaveStream;
        private LameMP3FileWriter _recordMp3Writer;
        private RecordState _recordState;
        private Einsatz _einsatz;

        #endregion //Private Properties

        #region Private Funtions

        private void registerEvents()
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Register Events");

            _decoderService.FmsMessageReceived += decoderService_FmsMessageReceived;
            _decoderService.PagerMessageReceived += decoderService_PagerMessageReceived;
        }

        private void unregisterEvents()
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Unregister Events");

            _decoderService.FmsMessageReceived -= decoderService_FmsMessageReceived;
            _decoderService.PagerMessageReceived -= decoderService_PagerMessageReceived;
        }

        private void decoderService_FmsMessageReceived(object sender, FmsMessageEventArgs e)
        {
            if (e == null || e.Status != "C" || _recordState == RecordState.RequestedStop) return;

            try
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"FMS-Message({e.Identifier}->{e.Status})");

                if (_einsatz == null)
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Create new einsatz");

                    _einsatz = new Einsatz(App.Path_Record);
                    startRecording();
                }

                _einsatz.AddFms(e);

                timerStatusEnd_Reset();
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

        private void decoderService_PagerMessageReceived(object sender, PagerMessageEventArgs e)
        {
            if (e == null || _recordState == RecordState.RequestedStop) return;

            try
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"Pager-Message({e.Identifier})");

                if (_einsatz == null)
                {
                    _einsatz = new Einsatz(App.Path_Record);
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Create new einsatz");

                    startRecording();
                }

                _einsatz.AddPager(e);

                timerAlarmEnd_Reset();
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

        private void timerStatusEnd_Reset()
        {
            if (_timerAlarmEnd != null && _timerAlarmEnd.Enabled)
            {
                _timerAlarmEnd.Stop();
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "timerAlarmEnd -> Stop");
            }

            if (_timerRecordEnd != null && _timerRecordEnd.Enabled)
            {
                _timerRecordEnd.Stop();
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "timerRecordEnd -> Stop");
            }

            if (_timerStatusEnd != null)
            {
                _timerStatusEnd.Stop();
                _timerStatusEnd.Start();
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "timerStatusEnd -> Reset");
            }
        }

        private void timerStatusEnd_Elapsed(object sender, ElapsedEventArgs e)
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "_timerStatusEnd -> Elapsed");

            if (_einsatz == null)
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "no einsatz");
            }
            else if (_einsatz.PagerMessages.Count == 0)
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Reset einsatz -> no pagers");
                _einsatz = null;

                //Stop record
                stopRecording();
            }
            else
            {
                timerAlarmEnd_Reset();
            }
        }

        private void timerAlarmEnd_Reset()
        {
            if (_timerRecordEnd != null && _timerRecordEnd.Enabled)
            {
                _timerRecordEnd.Stop();
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "timerRecordEnd -> Stop");
            }

            if (_timerAlarmEnd != null)
            {
                _timerAlarmEnd.Stop();
                _timerAlarmEnd.Start();
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "timerAlarmEnd -> Reset");
            }
        }

        private void timerAlarmEnd_Elapsed(object sender, ElapsedEventArgs e)
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "timerAlarmEnd -> Elapsed");

            if (_einsatz == null)
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "no einsatz");
            }
            else if (_einsatz.PagerMessages.Count == 0)
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Reset einsatz -> no pagers");
                _einsatz = null;

                //Stop record
                stopRecording();
            }
            else
            {
                _timerRecordEnd.Start();
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "timerRecordEnd -> Start");
            }
        }

        private void timerRecordEnd_Elapsed(object sender, ElapsedEventArgs e)
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "timerRecordEnd -> Elapsed");

            //Stop record                        
            stopRecording();
        }

        private void startRecording()
        {
            try
            {
                if (_einsatz == null) return;

                //Record Device
                _recordWaveStream = new WaveInEvent
                {
                    DeviceNumber = 0,
                    WaveFormat = new WaveFormat(44100, 16, 2)
                };
                _recordWaveStream.DataAvailable += recordDevice_DataAvailable;
                _recordWaveStream.RecordingStopped += recordDevice_RecordingStopped;
                _recordWaveStream.StartRecording();

                _recordMp3Writer = new LameMP3FileWriter(_einsatz.RecordPath, _recordWaveStream.WaveFormat, 32);
                _recordState = RecordState.Recording;

                Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"recordDevice -> Started({_einsatz.RecordPath})");
            }
            catch (Exception)
            {
                ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs
                {
                    Methode = MethodBase.GetCurrentMethod(),
                    Message = "recordDevice -> error on start"
                });
            }
        }

        private void stopRecording()
        {
            try
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "recordDevice -> StopRecording");

                if (_recordWaveStream != null)
                {
                    _recordState = RecordState.RequestedStop;
                    _recordWaveStream.StopRecording();
                }
                else
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "recordDevice -> already disposed");

                    recordDevice_RecordingStopped(this, null);
                }
            }
            catch (Exception ex)
            {
                ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs
                {
                    Methode = MethodBase.GetCurrentMethod(),
                    Error = ex
                });

                recordDevice_RecordingStopped(this, null);
            }
        }

        private void recordDevice_DataAvailable(object sender, WaveInEventArgs e)
        {
            try
            {
                if (_recordWaveStream == null || _recordMp3Writer == null) return;

                _recordMp3Writer.Write(e.Buffer, 0, e.BytesRecorded);
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

        private void recordDevice_RecordingStopped(object sender, StoppedEventArgs e)
        {
            try
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "recordDevice -> Stopped");

                if (_einsatz != null)
                {
                    //Raise Event
                    EinsatzFinished.RaiseEvent(this, new EinsatzFinishedEventArgs(_einsatz));
                    //Reset Einsatz
                    _einsatz = null;
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Reset einsatz -> finish");
                }

                _recordMp3Writer?.Dispose();
                _recordWaveStream?.Dispose();
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
                _recordState = RecordState.Stopped;
                _recordWaveStream = null;
                _recordMp3Writer = null;
            }
        }

        #endregion //Private Funtions
    }
}