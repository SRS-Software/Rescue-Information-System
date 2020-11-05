#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using Limilabs.Client.IMAP;
using Limilabs.Client.SMTP;
using Limilabs.Mail;
using Limilabs.Mail.Headers;
using RIS.Business;
using RIS.Core.Ams;
using RIS.Core.Fax;
using RIS.Core.Helper;
using RIS.Core.Mail;
using RIS.Model;
using RIS.Properties;
using SRS.Utilities;
using SRS.Utilities.Extensions;
using MailAddress = System.Net.Mail.MailAddress;
using Timer = System.Timers.Timer;

#endregion

namespace RIS.Core
{
    public class MailService : IMailService
    {
        private readonly IAmsService _amsService;
        private readonly IBusiness _business;
        private readonly IFaxService _faxService;

        public MailService(IBusiness business, IAmsService amsService, IFaxService faxService)
        {
            try
            {
                _business = business;
                _amsService = amsService;
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

        #region Private Properties

        private CancellationTokenSource _receiveMailTaskTokenSource;
        private Task _receiveMailTask;
        private Timer _receiveMailTaskMonitorTimer;
        private DateTime _nextMailTaskRestartTime;

        #endregion //Private Properties

        #region Public Funtions

        public void Start()
        {
            try
            {
                IsRunning = false;

                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Starting");
                var _stopWatch = new Stopwatch();
                _stopWatch.Start();

                StartReceiveMail();

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

                _receiveMailTaskMonitorTimer?.Stop();
                _receiveMailTaskTokenSource?.Cancel();

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
        public event EventHandler<MailReceivedEventArgs> MailReceived;

        #endregion //Events

        #region Private Funtions

        private void registerEvents()
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Register Events");

            _amsService.EinsatzFinished += amsService_EinsatzFinished;
            _faxService.EinsatzCreated += faxService_EinsatzCreated;
        }

        private void unregisterEvents()
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Unregister Events");

            _amsService.EinsatzFinished -= amsService_EinsatzFinished;
            _faxService.EinsatzCreated -= faxService_EinsatzCreated;
        }

        private void amsService_EinsatzFinished(object sender, EinsatzFinishedEventArgs e)
        {
            if (e == null || e.Einsatz == null || e.Einsatz.PagerMessages == null) return;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    //Create pager list
                    var _pagerList = new List<Pager>();
                    foreach (var _pagerMessage in e.Einsatz.PagerMessages) _pagerList.Add(_pagerMessage.Pager);

                    var _message = new Message();
                    _message.Subject = Settings.Default.MailAms_Subject;

                    //Empfänger
                    _message.Recivers = new List<User>();
                    _message.Recivers.AddRange(_business.GetUserByPagers(_pagerList)
                        .Where(u => u.AlarmMessageService_RecordOn == false));

                    //Nachricht     
                    _message.Text = "Folgender Alarm wurde am " + e.Einsatz.AlarmTime.ToShortDateString() + " um " +
                                    e.Einsatz.AlarmTime.ToShortTimeString() + " ausgelöst:";
                    _message.Text += Environment.NewLine;
                    _message.Text += Environment.NewLine;

                    if (e.Einsatz.FmsMessages.Count > 0)
                    {
                        _message.Text += "Fahrzeuge:" + Environment.NewLine;
                        foreach (var _fms in e.Einsatz.FmsMessages)
                        {
                            var _vehicle = _business.GetVehicleByBosIdentifier(_fms.Identifier);
                            if (_vehicle == null) continue;

                            _message.Text += _vehicle.Name + Environment.NewLine;
                        }
                    }

                    if (_pagerList.Count > 0)
                    {
                        _message.Text += Environment.NewLine;
                        _message.Text += "Organisationen:" + Environment.NewLine;
                        foreach (var _pager in _pagerList)
                            _message.Text += _pager.Identifier + " - " + _pager.Name + Environment.NewLine;
                    }

                    //Start to send mail without attachment
                    sendMail(_message);

                    //Empfänger
                    _message.Recivers = new List<User>();
                    _message.Recivers.AddRange(_business.GetUserByPagers(_pagerList)
                        .Where(u => u.AlarmMessageService_RecordOn).ToList());

                    //Add Audio as attachment
                    _message.AttachmentPath = e.Einsatz.RecordPath;

                    //Start to send mail with attachment
                    sendMail(_message);
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

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var _message = new Message();
                    _message.Subject = Settings.Default.MailOutput_Subject;

                    //Empfänger          
                    _message.Recivers = new List<User>();
                    _message.Recivers.AddRange(_business.GetUserWithFaxMessageServiceMailOn().ToList());

                    //Nachricht     
                    _message.Text = "EINSATZDATEN:" + Environment.NewLine;
                    if (!string.IsNullOrEmpty(e.Einsatz.Schlagwort))
                        _message.Text += "Schlagwort: " + e.Einsatz.Schlagwort + Environment.NewLine;

                    if (!string.IsNullOrEmpty(e.Einsatz.Stichwort))
                        _message.Text += "Stichwort: " + e.Einsatz.Stichwort + Environment.NewLine;

                    if (!string.IsNullOrEmpty(e.Einsatz.Objekt))
                        _message.Text += "Objekt: " + e.Einsatz.Objekt + Environment.NewLine;

                    if (!string.IsNullOrEmpty(e.Einsatz.Straße))
                        _message.Text += "Straße: " + e.Einsatz.Straße + " " + e.Einsatz.Hausnummer +
                                         Environment.NewLine;

                    if (!string.IsNullOrEmpty(e.Einsatz.Ort))
                        _message.Text += "Ort: " + e.Einsatz.Ort + Environment.NewLine;

                    if (!string.IsNullOrEmpty(e.Einsatz.Abschnitt))
                        _message.Text += "Abschnitt: " + e.Einsatz.Abschnitt + Environment.NewLine;

                    if (!string.IsNullOrEmpty(e.Einsatz.Kreuzung))
                        _message.Text += "Kreuzung: " + e.Einsatz.Kreuzung + Environment.NewLine;

                    if (!string.IsNullOrEmpty(e.Einsatz.Station))
                        _message.Text += "Station: " + e.Einsatz.Station + Environment.NewLine;

                    if (!string.IsNullOrEmpty(e.Einsatz.Bemerkung))
                        _message.Text += "Bemerkung: " + e.Einsatz.Bemerkung + Environment.NewLine;

                    _message.Text += Environment.NewLine;
                    _message.Text += Environment.NewLine;
                    _message.Text += "EINSATZMITTEL:" + Environment.NewLine;
                    foreach (var _vehicle in e.Einsatz.Einsatzmittel)
                        _message.Text += _vehicle.Name + Environment.NewLine;

                    _message.Text += Environment.NewLine;
                    _message.Text += Environment.NewLine;
                    var coordinaten = e.Einsatz.KoordinatenWGS84();
                    if (coordinaten != null)
                    {
                        var nfi = new NumberFormatInfo {NumberDecimalSeparator = "."};
                        var googleMapsUrl =
                            $"https://www.google.com/maps/search/?api=1&query={coordinaten.Latitude.ToString(nfi)},{coordinaten.Longitude.ToString(nfi)}";
                        _message.Text += $"ROUTE: {Environment.NewLine}{googleMapsUrl}" + Environment.NewLine;
                    }
                    else if (!string.IsNullOrEmpty(e.Einsatz.Straße) && !string.IsNullOrEmpty(e.Einsatz.Hausnummer) &&
                             !string.IsNullOrEmpty(e.Einsatz.Ort))
                    {
                        var googleMapsQuery = $"{e.Einsatz.Straße} {e.Einsatz.Hausnummer}, {e.Einsatz.Ort}";
                        var googleMapsUrl =
                            $"https://www.google.com/maps/search/?api=1&query={HttpUtility.UrlEncode(googleMapsQuery)}";
                        _message.Text += $"ROUTE: {Environment.NewLine}{googleMapsUrl}" + Environment.NewLine;
                    }

                    //Start to send mail without attachment
                    sendMail(_message);


                    //Empfänger          
                    _message.Recivers = new List<User>();
                    _message.Recivers.AddRange(_business.GetUserWithFaxMessageServiceFaxOn().ToList());

                    //Add Faximage as attachment
                    var _attachmentPath = e.Einsatz.FaxPath;
                    if (Path.GetExtension(e.Einsatz.FaxPath).ToLower() == ".tif")
                    {
                        _attachmentPath =
                            $"{Settings.Default.WorkingFolder}\\Temp\\ALARMFAX_PNG_{e.Einsatz.AlarmTime.ToString("yyyy-MM-dd_HH-mm-ss")}.png";
                        TiffConverter.SaveTiffAsImage(e.Einsatz.FaxPath, _attachmentPath);
                    }

                    _message.AttachmentPath = _attachmentPath;

                    //Start to send mail with attachment
                    sendMail(_message);
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

        private void sendMail(Message _message)
        {
            if (_message == null) return;

            if (string.IsNullOrWhiteSpace(Settings.Default.MailOutput_Server)) return;

            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Start to send mail");

            //Check settings
            if (Settings.Default.MailOutput_Port <= 0) throw new ArgumentNullException("MailOutput_Port");

            if (string.IsNullOrWhiteSpace(Settings.Default.MailOutput_Sender))
                throw new ArgumentNullException("MailOutput_Sender");

            if (string.IsNullOrWhiteSpace(Settings.Default.MailOutput_User))
                throw new ArgumentNullException("MailOutput_User");

            if (string.IsNullOrWhiteSpace(Settings.Default.MailOutput_Password))
                throw new ArgumentNullException("MailOutput_Password");

            //Settings
            var _senderMailbox = new MailBox(Settings.Default.MailOutput_Sender, "Rescue-Information-System");
            var _mailBuilder = new MailBuilder();
            _mailBuilder.From.Add(_senderMailbox);
            _mailBuilder.Subject = _message.Subject;
            _mailBuilder.Text = _message.Text;
            //Werbung
            for (var i = 0; i < 5; i++) _mailBuilder.Text += Environment.NewLine;

            _mailBuilder.Text += "RIS by www.srs-software.de";

            //Empfänger hinzufügen 
            foreach (var _user in _message.Recivers)
            {
                if (!isValidEmail(_user.MailAdresse))
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"User {_user.Name} -> mail address not valid");
                    continue;
                }

                var _newAdress = new MailBox(_user.MailAdresse, _user.Name);
                _mailBuilder.Bcc.Add(_newAdress);
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"User add ({_user.Name})");
            }

            if (_mailBuilder.Bcc.Count <= 0)
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "cancel no recipients");
                return;
            }

            //Anhang      
            if (!string.IsNullOrEmpty(_message.AttachmentPath))
            {
                if (File.Exists(_message.AttachmentPath) && WaitFileReady.Check(_message.AttachmentPath))
                    _mailBuilder.AddAttachment(_message.AttachmentPath);
                else
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"{_message.AttachmentPath} -> not found");
            }

            //send mail          
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "start sending");
            using (var _smtpClient = new Smtp())
            {
                if (Settings.Default.MailOutput_SSL)
                {
                    _smtpClient.ConnectSSL(Settings.Default.MailOutput_Server, Settings.Default.MailOutput_Port);
                }
                else
                {
                    _smtpClient.Connect(Settings.Default.MailOutput_Server, Settings.Default.MailOutput_Port);
                    if (_smtpClient.SupportedExtensions().Contains(SmtpExtension.StartTLS)) _smtpClient.StartTLS();
                }

                _smtpClient.UseBestLogin(Settings.Default.MailOutput_User,
                    Encrypt.DecryptString(Settings.Default.MailOutput_Password, "MailOutput_Password"));
                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "login -> ok");

                var _mail = _mailBuilder.Create();
                _mail.PriorityHigh();
                var _result = _smtpClient.SendMessage(_mail);

                Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"result -> {_result}");
                _smtpClient.Close();
            }

            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "send mail finished");
        }

        private bool isValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void StartReceiveMail()
        {
            if (string.IsNullOrEmpty(Settings.Default.MailInput_Server))
            {
                Logger.WriteDebug("MailService: Server not set -> stop receive");
                return;
            }

            if (Settings.Default.MailInput_Port <= 0)
            {
                Logger.WriteDebug("MailService: Port not set -> stop receive");
                return;
            }

            if (string.IsNullOrWhiteSpace(Settings.Default.MailInput_User))
            {
                Logger.WriteDebug("MailService: User not set -> stop receive");
                return;
            }

            if (string.IsNullOrWhiteSpace(Settings.Default.MailInput_Password))
            {
                Logger.WriteDebug("MailService: Password not set -> stop receive");
                return;
            }

            _receiveMailTaskTokenSource = new CancellationTokenSource();
            _nextMailTaskRestartTime = DateTime.Now.AddMinutes(1);
            _receiveMailTask = Task.Factory.StartNew(() =>
            {
                _receiveMailTaskTokenSource.Token.ThrowIfCancellationRequested();
                Logger.WriteDebug("MailService.receiveTask: start");

                try
                {
                    //Check settings
                    using (var imapClient = new Imap())
                    {
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Connect to IMAP-Server...");

                        //Connect
                        try
                        {
                            if (Settings.Default.MailInput_SSL)
                                imapClient.ConnectSSL(Settings.Default.MailInput_Server,
                                    Settings.Default.MailInput_Port);
                            else
                                imapClient.Connect(Settings.Default.MailInput_Server, Settings.Default.MailInput_Port);

                            imapClient.UseBestLogin(Settings.Default.MailInput_User,
                                Encrypt.DecryptString(Settings.Default.MailInput_Password, "MailInput_Password"));
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Connect -> ok");
                        }
                        catch (Exception ex)
                        {
                            ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs
                            {
                                Methode = MethodBase.GetCurrentMethod(),
                                Error = ex
                            });

                            _receiveMailTaskMonitorTimer?.Stop();
                            _receiveMailTaskTokenSource?.Cancel();
                            return;
                        }

                        //Chech IDLE command
                        if (!imapClient.SupportedExtensions().Contains(ImapExtension.Idle))
                        {
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                                "Server does not support imap idle -> abort");
                            return;
                        }

                        //Select standart Inbox as folder
                        imapClient.SelectInbox();

                        //Thread to recognize stop command and stop idle command
                        Task.Factory.StartNew(() =>
                        {
                            Logger.WriteDebug("MailService.idelTask: start");

                            while (!_receiveMailTaskTokenSource.Token.IsCancellationRequested) Thread.Sleep(1000);

                            imapClient.StopIdle();

                            Logger.WriteDebug("MailService.idelTask: stop");
                        }, _receiveMailTaskTokenSource.Token);

                        //Thread hangs until new mail or stop
                        while (!_receiveMailTaskTokenSource.Token.IsCancellationRequested)
                        {
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "imap idle -> start");
                            var currentStatus = imapClient.Idle();

                            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "imap idle -> finished");
                            if (_receiveMailTaskTokenSource.Token.IsCancellationRequested)
                                break;

                            //Create expression
                            var _searchExpression = Expression.HasFlag(Flag.Unseen);
                            if (!string.IsNullOrWhiteSpace(Settings.Default.MailInput_Subject))
                                _searchExpression =
                                    Expression.And(Expression.Subject(Settings.Default.MailInput_Subject),
                                        Expression.HasFlag(Flag.Unseen));

                            //Query messages uids 
                            foreach (var uid in imapClient.Search(_searchExpression))
                            {
                                Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"message UID -> {uid}");
                                imapClient.MarkMessageSeenByUID(uid);

                                var mailBuilder = new MailBuilder();
                                mailBuilder.CreatePlainTextAutomatically = true;

                                var emlMessage = imapClient.GetMessageByUID(uid);
                                var mail = mailBuilder.CreateFromEml(emlMessage);

                                if (mail.Attachments.Count >= 1)
                                {
                                    Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                                        $"message attachments -> {mail.Attachments.Count}");

                                    // save all attachments to disk
                                    var attachmentId = 1;
                                    foreach (var mime in mail.Attachments)
                                    {
                                        mime.Save(Path.Combine(Settings.Default.Fax_PathInput,
                                            $"ALARMMAIL-ATTACHMENT-{attachmentId}_{Guid.NewGuid()}{Path.GetExtension(mime.SafeFileName)}"));
                                        Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                                            $"message attachment [{mime.SafeFileName}] -> saved");
                                        attachmentId++;
                                    }
                                }
                                else
                                {
                                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"message text -> {mail.Text}");
                                    if (string.IsNullOrWhiteSpace(mail.Text))
                                    {
                                        Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                                            "message text is empty -> abort");
                                        continue;
                                    }

                                    //Write message to txt file
                                    File.WriteAllText(
                                        Path.Combine(Settings.Default.Fax_PathInput, $"ALARMMAIL_{Guid.NewGuid()}.txt"),
                                        mail.Text, Encoding.GetEncoding(1252));

                                    //Raise EinsatzCreated
                                    MailReceived.RaiseEvent(this, new MailReceivedEventArgs(mail.Text));
                                }
                            }
                        }

                        //Disconnect
                        imapClient.Close();
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

                Logger.WriteDebug("MailService.receiveTask: stop");
            }, _receiveMailTaskTokenSource.Token);

            //Start task monitor if mail task is running
            if (_receiveMailTask != null && _receiveMailTask.Status == TaskStatus.Running)
            {
                Logger.WriteDebug("MailService.receiveMailTaskMonitorTimer: start");
                _receiveMailTaskMonitorTimer = new Timer();
                _receiveMailTaskMonitorTimer.Elapsed += receiveMailTaskMonitorTimer_Elapsed;
                _receiveMailTaskMonitorTimer.Interval = 5000;
                _receiveMailTaskMonitorTimer.Start();
            }
        }

        private void receiveMailTaskMonitorTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_receiveMailTask.Status == TaskStatus.Running || DateTime.Now < _nextMailTaskRestartTime)
                return;
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "receiveMailTask -> not running");

            //Stop running task
            _receiveMailTaskTokenSource?.Cancel();

            //Restart receive task
            StartReceiveMail();
        }

        #endregion //Private Funtions
    }
}