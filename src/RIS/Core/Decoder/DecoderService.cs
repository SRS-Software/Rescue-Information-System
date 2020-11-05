#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using RIS.Business;
using RIS.Core.Decoder;
using RIS.Core.Helper;
using RIS.Properties;
using SRS.Utilities;
using SRS.Utilities.Extensions;

#endregion

namespace RIS.Core
{
    public class DecoderService : IDecoderService
    {
        private static readonly string ETX_FMS32 = "\r\n";
        private static readonly string ETX_MONITORD = "\r\n";
        private static readonly string ETX_SANDAN = "\r\n";
        private static readonly string ETX_OPERATOR2 = "\r";
        private static readonly string ETX_SYSLOG = "\n";
        private static readonly string ETX_LARDIS = "\n";

        private readonly IBusiness _business;

        public DecoderService(IBusiness business)
        {
            try
            {
                _business = business;

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
                var _stopWatch = new Stopwatch();
                _stopWatch.Start();

                IsRunning = false;

                //Timeout between messages 
                _delayTimer = new Timer
                {
                    Interval = 2500,
                    AutoReset = false
                };
                _delayTimer.Elapsed += delayTimer_Elapsed;
                _delayTimer.Start();

                if (Settings.Default.Decoder1_Mode != DecoderMode.OFF)
                {
                    //Create DecoderInstance
                    switch (Settings.Default.Decoder1_Mode)
                    {
                        case DecoderMode.FMS32:
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "[1]FMS32");
                            _decoderClient1 = new DecoderTcpClient("DecoderService[1]",
                                Settings.Default.Decoder1_Server, Settings.Default.Decoder1_Port, ETX_FMS32,
                                Encoding.ASCII);
                            break;
                        case DecoderMode.MONITORD:
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "[1]MONITORD");
                            _decoderClient1 = new DecoderTcpClient("DecoderService[1]",
                                Settings.Default.Decoder1_Server, Settings.Default.Decoder1_Port, ETX_MONITORD,
                                Encoding.ASCII);
                            break;
                        case DecoderMode.SANDAN:
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "[1]SANDAN");
                            _decoderClient1 = new DecoderTcpClient("DecoderService[1]",
                                Settings.Default.Decoder1_Server, Settings.Default.Decoder1_Port, ETX_SANDAN,
                                Encoding.ASCII);
                            _decoderClient1.EnableLoginString("GET NidanConnect!34");
                            break;
                        case DecoderMode.OPERATOR2:
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "[1]OPERATOR2");
                            _decoderClient1 = new DecoderTcpClient("DecoderService[1]",
                                Settings.Default.Decoder1_Server, Settings.Default.Decoder1_Port, ETX_OPERATOR2,
                                Encoding.ASCII);
                            _decoderClient1.EnableLoginString("OPERATOR2RIS");
                            break;
                        case DecoderMode.SYSLOG:
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "[1]SYSLOG");
                            _decoderClient1 = new DecoderTcpServer("DecoderService[1]",
                                Settings.Default.Decoder1_Server, Settings.Default.Decoder1_Port, ETX_SYSLOG);
                            break;
                        case DecoderMode.LARDIS:
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "[1]LARDIS");
                            _decoderClient1 = new DecoderTcpClient("DecoderService[1]",
                                Settings.Default.Decoder1_Server, Settings.Default.Decoder1_Port, ETX_LARDIS,
                                Encoding.GetEncoding(1252));
                            break;
                        case DecoderMode.TETRACONTROL:
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "[1]TETRACONTROL");
                            _decoderClient1 =
                                new DecoderHttpServer("DecoderService[1]", Settings.Default.Decoder1_Port);
                            break;
                    }

                    //Register for DecoderClient1 events
                    _decoderClient1.Connecting += (sender, e) =>
                    {
                        var _statusEventArgs = new StatusEventArgs
                        {
                            Number = 1,
                            Status = "Verbinde..."
                        };
                        StatusChanged.RaiseEvent(this, _statusEventArgs);
                    };
                    _decoderClient1.Connected += (sender, e) =>
                    {
                        var _statusEventArgs = new StatusEventArgs
                        {
                            Number = 1,
                            Status = "Verbunden"
                        };
                        StatusChanged.RaiseEvent(this, _statusEventArgs);
                    };
                    _decoderClient1.Disconnected += (sender, e) =>
                    {
                        var _statusEventArgs = new StatusEventArgs
                        {
                            Number = 1,
                            Status = "Getrennt"
                        };
                        StatusChanged.RaiseEvent(this, _statusEventArgs);
                    };
                    _decoderClient1.MessageReceived += (sender, e) =>
                    {
                        switch (Settings.Default.Decoder1_Mode)
                        {
                            case DecoderMode.FMS32:
                                decoderClient_MessageReceived_FMS32(e);
                                break;
                            case DecoderMode.MONITORD:
                                decoderClient_MessageReceived_MONITORD(e);
                                break;
                            case DecoderMode.SANDAN:
                                decoderClient_MessageReceived_SANDAN(e);
                                break;
                            case DecoderMode.OPERATOR2:
                                decoderClient_MessageReceived_OPERATOR2(e);
                                break;
                            case DecoderMode.SYSLOG:
                                decoderClient_MessageReceived_SYSLOG(e);
                                break;
                            case DecoderMode.LARDIS:
                                decoderClient_MessageReceived_LARDIS(e);
                                break;
                            case DecoderMode.TETRACONTROL:
                                decoderClient_MessageReceived_TETRACONTROL(e);
                                break;
                        }
                    };

                    //Start decoder
                    _decoderClient1.Start();

                    //Send message after connecting
                    switch (Settings.Default.Decoder1_Mode)
                    {
                        case DecoderMode.SANDAN:
                            _decoderClient1.EnablePingTimer("Ping", 15000);
                            break;
                    }
                }
                else
                {
                    var _statusEventArgs = new StatusEventArgs
                    {
                        Number = 1,
                        Status = "Deaktiviert"
                    };
                    StatusChanged.RaiseEvent(this, _statusEventArgs);
                }


                if (Settings.Default.Decoder2_Mode != DecoderMode.OFF)
                {
                    //Create DecoderClient
                    switch (Settings.Default.Decoder2_Mode)
                    {
                        case DecoderMode.FMS32:
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "[2]FMS32");
                            _decoderClient2 = new DecoderTcpClient("DecoderService[2]",
                                Settings.Default.Decoder2_Server, Settings.Default.Decoder2_Port, ETX_FMS32,
                                Encoding.ASCII);
                            break;
                        case DecoderMode.MONITORD:
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "[2]MONITORD");
                            _decoderClient2 = new DecoderTcpClient("DecoderService[2]",
                                Settings.Default.Decoder2_Server, Settings.Default.Decoder2_Port, ETX_MONITORD,
                                Encoding.ASCII);
                            break;
                        case DecoderMode.SANDAN:
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "[2]SANDAN");
                            _decoderClient2 = new DecoderTcpClient("DecoderService[2]",
                                Settings.Default.Decoder2_Server, Settings.Default.Decoder2_Port, ETX_SANDAN,
                                Encoding.ASCII);
                            _decoderClient2.EnableLoginString("GET NidanConnect!34");
                            break;
                        case DecoderMode.OPERATOR2:
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "[2]OPERATOR2");
                            _decoderClient2 = new DecoderTcpClient("DecoderService[2]",
                                Settings.Default.Decoder2_Server, Settings.Default.Decoder2_Port, ETX_OPERATOR2,
                                Encoding.ASCII);
                            _decoderClient2.EnableLoginString("OPERATOR2RIS");
                            break;
                        case DecoderMode.SYSLOG:
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "[2]SYSLOG");
                            _decoderClient2 = new DecoderTcpServer("DecoderService[2]",
                                Settings.Default.Decoder2_Server, Settings.Default.Decoder2_Port, ETX_SYSLOG);
                            break;
                        case DecoderMode.LARDIS:
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "[2]LARDIS");
                            _decoderClient2 = new DecoderTcpClient("DecoderService[2]",
                                Settings.Default.Decoder2_Server, Settings.Default.Decoder2_Port, ETX_LARDIS,
                                Encoding.GetEncoding(1252));
                            break;
                        case DecoderMode.TETRACONTROL:
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "[2]TETRACONTROL");
                            _decoderClient2 =
                                new DecoderHttpServer("DecoderService[2]", Settings.Default.Decoder2_Port);
                            break;
                    }

                    //Register for DecoderClient2 events
                    _decoderClient2.Connecting += (sender, e) =>
                    {
                        var _statusEventArgs = new StatusEventArgs
                        {
                            Number = 2,
                            Status = "Verbinde..."
                        };
                        StatusChanged.RaiseEvent(this, _statusEventArgs);
                    };
                    _decoderClient2.Connected += (sender, e) =>
                    {
                        var _statusEventArgs = new StatusEventArgs
                        {
                            Number = 2,
                            Status = "Verbunden"
                        };
                        StatusChanged.RaiseEvent(this, _statusEventArgs);
                    };
                    _decoderClient2.Disconnected += (sender, e) =>
                    {
                        var _statusEventArgs = new StatusEventArgs
                        {
                            Number = 2,
                            Status = "Getrennt"
                        };
                        StatusChanged.RaiseEvent(this, _statusEventArgs);
                    };
                    _decoderClient2.MessageReceived += (sender, e) =>
                    {
                        switch (Settings.Default.Decoder2_Mode)
                        {
                            case DecoderMode.FMS32:
                                decoderClient_MessageReceived_FMS32(e);
                                break;
                            case DecoderMode.MONITORD:
                                decoderClient_MessageReceived_MONITORD(e);
                                break;
                            case DecoderMode.SANDAN:
                                decoderClient_MessageReceived_SANDAN(e);
                                break;
                            case DecoderMode.OPERATOR2:
                                decoderClient_MessageReceived_OPERATOR2(e);
                                break;
                            case DecoderMode.SYSLOG:
                                decoderClient_MessageReceived_SYSLOG(e);
                                break;
                            case DecoderMode.LARDIS:
                                decoderClient_MessageReceived_LARDIS(e);
                                break;
                            case DecoderMode.TETRACONTROL:
                                decoderClient_MessageReceived_TETRACONTROL(e);
                                break;
                        }
                    };

                    //Start Decoder
                    _decoderClient2.Start();

                    //Send message after connecting
                    switch (Settings.Default.Decoder2_Mode)
                    {
                        case DecoderMode.SANDAN:
                            _decoderClient2.EnablePingTimer("Ping", 15000);
                            break;
                    }
                }
                else
                {
                    var _statusEventArgs = new StatusEventArgs
                    {
                        Number = 2,
                        Status = "Deaktiviert"
                    };
                    StatusChanged.RaiseEvent(this, _statusEventArgs);
                }

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

                //Stop decoderClient1 if enabled
                if (_decoderClient1 != null)
                {
                    _decoderClient1.Stop();
                    _decoderClient1 = null;
                }

                //Stop decoderClient2 if enabled
                if (_decoderClient2 != null)
                {
                    _decoderClient2.Stop();
                    _decoderClient2 = null;
                }

                //Stop delayTimer if enabled
                if (_delayTimer != null)
                {
                    _delayTimer.Stop();
                    _delayTimer = null;
                }

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

        //public event EventHandler<ExceptionEventArgs> ExceptionOccured;
        public event EventHandler<ExceptionEventArgs> ExceptionOccured;
        public event EventHandler<StatusEventArgs> StatusChanged;
        public event EventHandler<FmsMessageEventArgs> FmsMessageReceived;
        public event EventHandler<PagerMessageEventArgs> PagerMessageReceived;

        #endregion //Events

        #region Private Properties

        private IDecoderClient _decoderClient1;
        private IDecoderClient _decoderClient2;

        private Timer _delayTimer;
        private readonly List<FmsMessageEventArgs> _fmsMessages = new List<FmsMessageEventArgs>();
        private readonly List<PagerMessageEventArgs> _pagerMessages = new List<PagerMessageEventArgs>();

        #endregion //Private Properties

        #region Private Funtions

        private void decoderClient_MessageReceived_FMS32(MessageEventArgs e)
        {
            try
            {
                var _messageFields = e.Data.Split('\t');
                if (_messageFields == null || _messageFields.Count() <= 0) return;

                switch (_messageFields[0])
                {
                    case "FMSTlg":
                        if (_messageFields.Count() != 16) return;

                        //Convert Directionstatus ILS->FZG
                        if (_messageFields[8] == "1")
                            switch (_messageFields[6])
                            {
                                case "1":
                                    _messageFields[6] = "A";
                                    break;
                                case "2":
                                    _messageFields[6] = "E";
                                    break;
                                case "3":
                                    _messageFields[6] = "C";
                                    break;
                                case "4":
                                    _messageFields[6] = "F";
                                    break;
                                case "5":
                                    _messageFields[6] = "H";
                                    break;
                                case "6":
                                    _messageFields[6] = "J";
                                    break;
                                case "7":
                                    _messageFields[6] = "L";
                                    break;
                                case "8":
                                    _messageFields[6] = "P";
                                    break;
                                case "9":
                                    _messageFields[6] = "U";
                                    break;
                            }

                        fmsMessageReceived(_messageFields[1], _messageFields[6]);
                        break;
                    case "ZVEI":
                        if (_messageFields.Count() != 4) return;

                        pagerMessageReceived(_messageFields[1], null);
                        break;
                    case "POC":
                        if (_messageFields.Count() != 6) return;

                        pagerMessageReceived(_messageFields[1] + _messageFields[2], _messageFields[3]);
                        break;
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

        private void decoderClient_MessageReceived_MONITORD(MessageEventArgs e)
        {
            try
            {
                var _messageFields = e.Data.Split(':');
                if (_messageFields == null || _messageFields.Count() <= 0) return;

                switch (_messageFields[0])
                {
                    case "310":
                        if (_messageFields.Count() != 8) return;

                        //Convert Directionstatus ILS->FZG
                        if (_messageFields[6] == "1")
                            switch (_messageFields[4])
                            {
                                case "1":
                                    _messageFields[4] = "A";
                                    break;
                                case "2":
                                    _messageFields[4] = "E";
                                    break;
                                case "3":
                                    _messageFields[4] = "C";
                                    break;
                                case "4":
                                    _messageFields[4] = "F";
                                    break;
                                case "5":
                                    _messageFields[4] = "H";
                                    break;
                                case "6":
                                    _messageFields[4] = "J";
                                    break;
                                case "7":
                                    _messageFields[4] = "L";
                                    break;
                                case "8":
                                    _messageFields[4] = "P";
                                    break;
                                case "9":
                                    _messageFields[4] = "U";
                                    break;
                            }

                        fmsMessageReceived(_messageFields[3], _messageFields[4]);
                        break;
                    case "300":
                        if (_messageFields.Count() != 6) return;

                        pagerMessageReceived(_messageFields[3], null);
                        break;
                    case "320":
                        if (_messageFields.Count() != 6) return;

                        if (!string.IsNullOrEmpty(_messageFields[5]))
                        {
                            var _byteArray = Enumerable.Range(0, _messageFields[5].Length).Where(x => x % 2 == 0)
                                .Select(x => Convert.ToByte(_messageFields[5].Substring(x, 2), 16)).ToArray();
                            _messageFields[5] = new ASCIIEncoding().GetString(_byteArray);
                        }

                        pagerMessageReceived(_messageFields[3] + _messageFields[4], _messageFields[5]);
                        break;
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

        private void decoderClient_MessageReceived_SANDAN(MessageEventArgs e)
        {
            try
            {
                var _messageFields = e.Data.Split(';');
                if (_messageFields == null || _messageFields.Count() <= 0) return;

                switch (_messageFields[0])
                {
                    case "!FMS":
                        if (_messageFields.Count() != 6) return;

                        fmsMessageReceived(_messageFields[1], convertTetraStatusToFms(_messageFields[4]));
                        break;
                    case "!GPS":
                        break;
                    case "!SDS":
                        break;
                    case "!FAE":
                        break;
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

        private void decoderClient_MessageReceived_OPERATOR2(MessageEventArgs e)
        {
            try
            {
                var _messageFields = e.Data.Split(';');
                if (_messageFields == null || _messageFields.Count() <= 0) return;

                switch (_messageFields[0])
                {
                    case "!FMS":
                        //!FMS;6703xxx;HEFW.xx#xx219MTW......1.;32772;02.08.2014 17:15:29
                        if (_messageFields.Count() != 5) return;

                        fmsMessageReceived(_messageFields[1], convertTetraStatusToFms(_messageFields[3]));
                        break;
                    case "!SDS":
                        //!SDS;6703xxx;HEFW.xx#xx219MTW......1.;S;Test;02.08.2014 17:15:29
                        if (_messageFields.Count() != 6)
                        {
                        }

                        break;
                    case "!ZVEI":
                        //!ZVEI;20815;02.08.2014 17:15:29
                        if (_messageFields.Count() != 3) return;

                        pagerMessageReceived(_messageFields[1], null);
                        break;
                    case "!POCSAG":
                        //!POCSAG;1234567;1;Brand Wohnhaus;02.08.2014 17:15:29
                        if (_messageFields.Count() != 5) return;

                        pagerMessageReceived(_messageFields[1] + _messageFields[2], _messageFields[3]);
                        break;
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

        private void decoderClient_MessageReceived_SYSLOG(MessageEventArgs e)
        {
            try
            {
                if (Text.CheckString(e.Data, "ZVEI|"))
                {
                    var _zveiMessage = Text.FindString(e.Data, "ZVEI|").Substring(0, 5);
                    if (string.IsNullOrWhiteSpace(_zveiMessage)) return;

                    pagerMessageReceived(_zveiMessage, null);
                }
                else if (Text.CheckString(e.Data, "FMS|"))
                {
                    var _fmsMessage = Text.FindString(e.Data, "FMS|").Substring(0, 12);
                    if (string.IsNullOrWhiteSpace(_fmsMessage)) return;

                    var _fmsIdentifier = _fmsMessage.Substring(0, 8);
                    var _fmsStatus = _fmsMessage[8].ToString();
                    var _fmsDirection = _fmsMessage[10].ToString();
                    if (_fmsDirection == "1")
                        switch (_fmsStatus)
                        {
                            case "1":
                                _fmsStatus = "A";
                                break;
                            case "2":
                                _fmsStatus = "E";
                                break;
                            case "3":
                                _fmsStatus = "C";
                                break;
                            case "4":
                                _fmsStatus = "F";
                                break;
                            case "5":
                                _fmsStatus = "H";
                                break;
                            case "6":
                                _fmsStatus = "J";
                                break;
                            case "7":
                                _fmsStatus = "L";
                                break;
                            case "8":
                                _fmsStatus = "P";
                                break;
                            case "9":
                                _fmsStatus = "U";
                                break;
                        }

                    fmsMessageReceived(_fmsIdentifier, _fmsStatus);
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

        private void decoderClient_MessageReceived_LARDIS(MessageEventArgs e)
        {
            try
            {
                if (e == null || string.IsNullOrEmpty(e.Data))
                    Logger.WriteError(MethodBase.GetCurrentMethod(), "LARDIS -> MessageEventArgs not valid");

                Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"LARDIS -> {e.Data}");

                var regex = new Regex(@"(?<COMMAND>.*?)\:(?<DATA>.*)");
                var regexMatch = regex.Match(e.Data);
                if (!regexMatch.Success)
                    return;

                var command = regexMatch.Groups["COMMAND"]?.Value;
                if (command == null)
                {
                    Logger.WriteError(MethodBase.GetCurrentMethod(), "LARDIS -> command field not set");
                    return;
                }

                var data = regexMatch.Groups["DATA"]?.Value;
                if (data == null)
                {
                    Logger.WriteError(MethodBase.GetCurrentMethod(), "LARDIS -> data field not set");
                    return;
                }

                switch (command)
                {
                    case "DEVICE":
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"LARDIS -> receice device infos ({data})");
                        break;
                    case "Mail":
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"LARDIS -> receice text message ({data})");

                        var regexMailData = new Regex("(?<ID>.*?),(?<IDENTIFIER>.*),(?<TIME>.*),\"(?<SDSTEXT>.*)\"");
                        var regexMailMatch = regexMailData.Match(data);
                        if (!regexMailMatch.Success || regexMailMatch.Groups.Count < 2)
                        {
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                                "LARDIS -> text message parameter field not found");
                            return;
                        }

                        var sdsIdentifier = regexMailMatch.Groups["IDENTIFIER"]?.Value;
                        if (sdsIdentifier == null)
                        {
                            Logger.WriteError(MethodBase.GetCurrentMethod(),
                                "LARDIS -> text message sds identifier not found");
                            return;
                        }

                        var sdsText = regexMailMatch.Groups["SDSTEXT"]?.Value;
                        if (sdsText == null)
                        {
                            Logger.WriteError(MethodBase.GetCurrentMethod(),
                                "LARDIS -> text message sds text not found");
                            return;
                        }

                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"LARDIS -> extract sds text({sdsText})");

                        //Check if sds is a status message
                        var status = convertTetraStatusToFms(sdsText);
                        if (!string.IsNullOrEmpty(status))
                        {
                            fmsMessageReceived(sdsIdentifier, status);
                        }
                        else
                        {
                            //create fax text file
                            var faxFilePath =
                                $"{Settings.Default.Fax_PathInput}\\SDS_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.txt";
                            using (var _streamWriter = new StreamWriter(faxFilePath, false, Encoding.UTF8))
                            {
                                _streamWriter.Write(sdsText);
                            }
                        }

                        break;
                    case "RadioStatus":
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"LARDIS -> receice status message ({data})");

                        var regexRadioStatusData = new Regex("(?<IDENTIFIER>.*?),(?<STATUS>.*)");
                        var regexRadioStatusMatch = regexRadioStatusData.Match(data);
                        if (!regexRadioStatusMatch.Success || regexRadioStatusMatch.Groups.Count < 2)
                        {
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                                "LARDIS -> status message parameter field not found");
                            return;
                        }

                        var statusIdentifier = regexRadioStatusMatch.Groups["IDENTIFIER"]?.Value;
                        if (statusIdentifier == null)
                        {
                            Logger.WriteError(MethodBase.GetCurrentMethod(),
                                "LARDIS -> status message identifier not found");
                            return;
                        }

                        var statusText = regexRadioStatusMatch.Groups["STATUS"]?.Value;
                        if (statusText == null)
                        {
                            Logger.WriteError(MethodBase.GetCurrentMethod(),
                                "LARDIS -> status message text not found");
                            return;
                        }

                        fmsMessageReceived(statusIdentifier, convertTetraStatusToFms(statusText));
                        break;
                    //case "ZVEI": contains tailing F for Sirene
                    case "5TONE":
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"LARDIS -> receice 5TONE message ({data})");

                        pagerMessageReceived(data, null);
                        break;
                    case "FMS":
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"LARDIS -> receice FMS message ({data})");
                        break;
                    case "CalloutACK":
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                            $"LARDIS -> receice TETRA callout acknowledge ({data})");
                        break;
                    case "LIP":
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"LARDIS -> receice TETRA position ({data})");
                        break;
                    case "State":
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                            $"LARDIS -> receice enddevice TETRA state ({data})");
                        break;
                    case "BOS":
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                            $"LARDIS -> receice enddevice BOS state ({data})");
                        break;
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

        private void decoderClient_MessageReceived_TETRACONTROL(MessageEventArgs e)
        {
            try
            {
                var _messageFields = e.Data.Split('\t');
                if (_messageFields == null || _messageFields.Count() <= 0) return;

                switch (_messageFields[0])
                {
                    case "FMS":
                        if (_messageFields.Count() != 3) return;

                        fmsMessageReceived(_messageFields[1], _messageFields[2]);
                        break;
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

        private void fmsMessageReceived(string _identifier, string _status)
        {
            //Check if params are valid
            if (string.IsNullOrEmpty(_identifier) || string.IsNullOrEmpty(_status)) return;

            //Check if message locked
            var _message = _fmsMessages.Where(f => f.Identifier == _identifier).FirstOrDefault();
            if (_message != null)
            {
                //Message with same status return
                if (_message.Status == _status) return;

                //Message with different status remove from list
                _fmsMessages.Remove(_message);
            }

            //Create EventArg
            var _arg = new FmsMessageEventArgs(_identifier, _status);
            _fmsMessages.Add(_arg);

            //Query Database      
            var _vehicle = _business.GetVehicleByBosIdentifier(_identifier);
            if (_vehicle != null)
            {
                //Set database object
                _arg.Vehicle = _vehicle;

                //Execute App or Sound on C
                if (_status == "C" && Settings.Default.AppExecute_OnStatus)
                    Task.Factory.StartNew(() =>
                    {
                        //Execute App or Sound   
                        if (!string.IsNullOrWhiteSpace(_vehicle.File))
                        {
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"Execute -> {_vehicle.File}");
                            Execute.SoundOrApp(_vehicle.File);
                        }
                    });
            }

            //Raise Event   
            FmsMessageReceived.RaiseEvent(this, _arg);
        }

        private void pagerMessageReceived(string _identifier, string _message)
        {
            //Check if params are valid
            if (string.IsNullOrEmpty(_identifier)) return;

            //Check if message locked
            var _pagerMessage =
                _pagerMessages.Where(z => z.Identifier == _identifier).FirstOrDefault();
            if (_pagerMessage != null) return;

            //Create EventArg
            var _arg = new PagerMessageEventArgs(_identifier, _message);
            _pagerMessages.Add(_arg);

            //Query Database
            var _pager = _business.GetPagerByIdentifier(_identifier);
            if (_pager != null)
            {
                //Set database object
                _arg.Pager = _pager;

                //Execute App or Sound   
                Task.Factory.StartNew(() =>
                {
                    if (!string.IsNullOrWhiteSpace(_pager.File))
                    {
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"Execute -> {_pager.File}");
                        Execute.SoundOrApp(_pager.File);
                    }
                });
            }

            //Raise Event
            PagerMessageReceived.RaiseEvent(this, _arg);
        }

        private void delayTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_fmsMessages)
            {
                _fmsMessages.RemoveAll(z => DateTime.Now.AddSeconds(-10) >= z.Time);
            }

            lock (_pagerMessages)
            {
                _pagerMessages.RemoveAll(z => DateTime.Now.AddSeconds(-15) >= z.Time);
            }

            if (_delayTimer != null) _delayTimer.Start();
        }

        private string convertTetraStatusToFms(string status)
        {
            switch (status)
            {
                case "32770":
                    return "9";
                case "32771":
                    return "1";
                case "32772":
                    return "2";
                case "32773":
                    return "3";
                case "32774":
                    return "4";
                case "32775":
                    return "5";
                case "32776":
                    return "6";
                case "32777":
                    return "7";
                case "32778":
                    return "8";
                case "33010":
                    return "A";
                case "33012":
                    return "C";
                case "33013":
                    return "F";
                case "33014":
                    return "E";
                case "33015":
                    return "J";
                default:
                    return string.Empty;
            }
        }

        #endregion //Private Funtions
    }
}