#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using RIS.Business;
using RIS.Core.Alarmapp;
using RIS.Core.AlarmappApi;
using RIS.Core.Decoder;
using RIS.Core.Fax;
using RIS.Model;
using RIS.Properties;
using SRS.Utilities;
using SRS.Utilities.Extensions;

#endregion

namespace RIS.Core
{
    public class AlarmappService : IAlarmappService
    {
        private readonly IBusiness _business;
        private readonly IDecoderService _decoderService;
        private readonly IFaxService _faxService;
        private readonly Timer _timerAlarmEnd;

        public AlarmappService(IBusiness business, IDecoderService decoderService, IFaxService faxService)
        {
            try
            {
                _business = business;
                _decoderService = decoderService;
                _faxService = faxService;
                _timerAlarmEnd = new Timer //Zeit keine Alarm -> lösche aktuelle AlarmId
                {
                    Interval = TimeSpan.FromMinutes(1).TotalMilliseconds,
                    AutoReset = false
                };
                _timerAlarmEnd.Elapsed += timerAlarmEnd_Elapsed;

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
                _apiService = null;

                if (string.IsNullOrEmpty(Settings.Default.Alarmapp_ApiToken))
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "AuthToken not set");
                    return;
                }

                _apiService = new AlarmappApiService(Settings.Default.Alarmapp_ApiToken);
                timerAlarmEnd_Elapsed(this, null); // reset pagerlist & alarmId
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

        public bool ClearAlarmgroups()
        {
            try
            {
                _business.DeleteAlarmappAll();

                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Clear -> OK");
                return true;
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

        public bool Refresh(string apiToken, string organisationId)
        {
            try
            {
                if (string.IsNullOrEmpty(apiToken))
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "apiToken not set");
                    return false;
                }

                if (string.IsNullOrEmpty(organisationId))
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "organisationId not set");
                    return false;
                }

                _apiService = new AlarmappApiService(apiToken);

                //Check departments and alarmgroups
                var _alarmgroupsResult = _apiService.GetAlarmgroups(organisationId);
                if (_alarmgroupsResult == null)
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "GetAlarmgroups -> fail");
                    return false;
                }

                //Check alarmgroups
                var dbDepartment = _business.GetAlarmappDepartmentById(organisationId);
                if (dbDepartment == null)
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"add new department -> {organisationId}");
                    dbDepartment = new AlarmappDepartment();
                    dbDepartment.DepartmentId = organisationId;
                    dbDepartment.DepartmentName = organisationId; //we dont get a name at the moment
                    dbDepartment = _business.AddOrUpdateAlarmappDepartment(dbDepartment);
                }

                //dbDepartment.DepartmentName = department.name;
                _business.AddOrUpdateAlarmappDepartment(dbDepartment);

                foreach (var alarmgroup in _alarmgroupsResult)
                {
                    var dbGroup = _business.GetAlarmappGroupByGroupId(alarmgroup.id);
                    if (dbGroup == null)
                    {
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"add new alarmgroup -> {alarmgroup.name}");
                        dbGroup = new AlarmappGroup();
                        dbGroup.Department = dbDepartment;
                        dbGroup.GroupId = alarmgroup.id;
                        dbGroup.GroupName = alarmgroup.name;
                        dbGroup = _business.AddOrUpdateAlarmappGroup(dbGroup);
                    }

                    dbGroup.GroupName = alarmgroup.name;
                    _business.AddOrUpdateAlarmappGroup(dbGroup);
                }

                Settings.Default.Alarmapp_ApiToken = apiToken;
                Settings.Default.Alarmapp_OrganisationId = organisationId;
                Settings.Default.Save();

                Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Refresh -> OK");
                return true;
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

        public AlarmStatus GetAlarmStatus(string operationId)
        {
            if (string.IsNullOrEmpty(operationId)) return null;

            try
            {
                var result = new AlarmStatus();
                var participationsList = new List<Participation>();

                //Functionsgroups
                var functiongroupList = _apiService.GetFunctiongroups(Settings.Default.Alarmapp_OrganisationId);
                foreach (var functiongroup in functiongroupList)
                {
                    var alarmStatusFunctiongroup = new AlarmStatus.Functiongroup
                    {
                        Id = functiongroup.Id,
                        Name = functiongroup.Name,
                        Code = functiongroup.ShortKey,
                        Background = AlarmStatus.ConvertStringToBrush(functiongroup.Colors.Background),
                        Foreground = AlarmStatus.ConvertStringToBrush(functiongroup.Colors.Foreground)
                    };
                    result.Functiongroups.Add(alarmStatusFunctiongroup);
                }

                var alarmUserList = _apiService.GetAlarmParticipations(operationId);
                if (alarmUserList != null && alarmUserList.Count != 0)
                {
                    // status: 0=No / 1= Accepted / 2=Rejected
                    result.AlarmedUser = alarmUserList.Count;
                    result.AccpetedUser = alarmUserList.Where(x => x.Status == 1).Count();
                    result.RejectedUser = alarmUserList.Where(x => x.Status == 2).Count();
                }


                foreach (var alarmUser in alarmUserList)
                {
                    //Users
                    var user = result.Users.Where(u => u.Id == alarmUser.Id).FirstOrDefault();
                    if (user == null)
                    {
                        user = new AlarmStatus.User
                        {
                            Id = alarmUser.Id,
                            Name = alarmUser.User.ToString()
                        };
                        result.Users.Add(user);
                    }

                    user.Status = AlarmStatus.ConvertStringToUserStatus(alarmUser.Status);
                    user.Functiongroups = new List<AlarmStatus.Functiongroup>();
                    foreach (var functiongroup in alarmUser.function_groups)
                    {
                        var userFunctiongroup = user.Functiongroups.Where(f => f.Id == functiongroup).FirstOrDefault();
                        if (userFunctiongroup == null)
                        {
                            userFunctiongroup = new AlarmStatus.Functiongroup
                            {
                                Id = functiongroup,
                                Name = functiongroupList.Where(x => x.Id == functiongroup).FirstOrDefault()?.Name,
                                Code = functiongroupList.Where(x => x.Id == functiongroup).FirstOrDefault()?.ShortKey,
                                Background = AlarmStatus.ConvertStringToBrush(functiongroupList
                                    .Where(x => x.Id == functiongroup).FirstOrDefault()?.Colors.Background),
                                Foreground = AlarmStatus.ConvertStringToBrush(functiongroupList
                                    .Where(x => x.Id == functiongroup).FirstOrDefault()?.Colors.Foreground)
                            };
                            user.Functiongroups.Add(userFunctiongroup);
                        }

                        if (user.Status == AlarmStatus.UserStatus.Accpeted)
                        {
                            var functiongroupOverview =
                                result.Functiongroups.Where(f => f.Id == functiongroup).FirstOrDefault();
                            functiongroupOverview.UserCount = functiongroupOverview.UserCount + 1;
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                ExceptionOccured.RaiseEvent(this, new ExceptionEventArgs
                {
                    Methode = MethodBase.GetCurrentMethod(),
                    Error = ex
                });
                return null;
            }
        }

        #endregion //Public Funtions

        #region Events

        public event EventHandler<ExceptionEventArgs> ExceptionOccured;

        public event EventHandler<AlarmedEventArgs> Alarmed;
        //public event EventHandler<UpdatedEventArgs> Updated;

        #endregion //Events

        #region Private Properties

        private AlarmappApiService _apiService;
        private string _currentAlarmId;
        private List<Pager> _currentPagerList = new List<Pager>();

        #endregion //Private Properties

        #region Private Funtions

        private void registerEvents()
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Register Events");

            _decoderService.PagerMessageReceived += decoderService_PagerMessageReceived;
            _faxService.EinsatzCreated += faxService_EinsatzCreated;
        }

        private void unregisterEvents()
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Unregister Events");

            _decoderService.PagerMessageReceived -= decoderService_PagerMessageReceived;
            _faxService.EinsatzCreated -= faxService_EinsatzCreated;
        }

        private void timerAlarmEnd_Elapsed(object sender, ElapsedEventArgs e)
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "timerAlarmEnd -> Elapsed");
            _currentAlarmId = null;
            _currentPagerList = new List<Pager>();
        }

        private void timerAlarmEnd_Reset()
        {
            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "timerAlarmEnd -> Reset");
            _timerAlarmEnd.Stop();
            _timerAlarmEnd.Start();
        }

        private void decoderService_PagerMessageReceived(object sender, PagerMessageEventArgs e)
        {
            if (e == null || e.Pager == null) return;

            if (_apiService == null) return;

            timerAlarmEnd_Reset();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"Pager-Message({e.Identifier})");

                    if (!_currentPagerList.Contains(e.Pager))
                        _currentPagerList.Add(e.Pager);

                    if (e.Pager.AlarmappGroups?.Count == 0 && string.IsNullOrEmpty(_currentAlarmId))
                        return;

                    var alarmDto = new AlarmDto();
                    alarmDto.Alarm.Title = "EINSATZ";

                    // add alarmgroups to dto
                    foreach (var group in e.Pager.AlarmappGroups) alarmDto.Groups.Add(group.GroupId);

                    // add text to dto
                    alarmDto.Alarm.AlarmDetails.Texts = new List<string>();
                    foreach (var pager in _currentPagerList)
                        alarmDto.Alarm.AlarmDetails.Texts.Add($"{pager.Identifier} - {pager.Name}");

                    //Send alarm to backend  
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"create alarm -> {alarmDto}");

                    if (string.IsNullOrEmpty(_currentAlarmId))
                    {
                        var _alarmResult = _apiService.CreateAlarm(alarmDto);
                        if (_alarmResult == null || string.IsNullOrEmpty(_alarmResult.alarm_id))
                        {
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "create alarm -> ERROR");
                            return;
                        }

                        _currentAlarmId = _alarmResult.alarm_id;
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"create alarm -> alarmId[{_currentAlarmId}]");
                    }
                    else
                    {
                        var _alarmResult = _apiService.UpdateAlarm(_currentAlarmId, alarmDto);
                        if (_alarmResult == null || string.IsNullOrEmpty(_alarmResult.alarm_id))
                        {
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "update alarm -> ERROR");
                            return;
                        }

                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"update alarm -> alarmId[{_currentAlarmId}]");
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
            });
        }

        private void faxService_EinsatzCreated(object sender, EinsatzCreatedEventArgs e)
        {
            if (e == null || e.Einsatz == null) return;

            if (_apiService == null) return;

            timerAlarmEnd_Reset();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), "Fax-Message");

                    var alarmDto = new AlarmDto();

                    //Select alarmgroups
                    var _groups = _business.GetAlarmappGroupWithAlarmfax();
                    foreach (var group in _groups)
                        if (group.Vehicles.Count > 0 && group.OnlyWithPager == false)
                        {
                            var faxVehicleIds = e.Einsatz.Einsatzmittel.Select(faxVehicle => faxVehicle.Id);
                            var result = group.Vehicles.Where(groupVehicle => faxVehicleIds.Contains(groupVehicle.Id))
                                .ToList();
                            if (result != null && result.Count >= 1) alarmDto.Groups.Add(group.GroupId);
                        }
                        else if (group.Vehicles.Count == 0 && group.OnlyWithPager)
                        {
                            var pagerIds = _currentPagerList.Select(pager => pager.Id);
                            var result = group.Pagers.Where(groupPager => pagerIds.Contains(groupPager.Id)).ToList();
                            if (result != null && result.Count >= 1) alarmDto.Groups.Add(group.GroupId);
                        }
                        else if (group.Vehicles.Count > 0 && group.OnlyWithPager)
                        {
                            var faxVehicleIds = e.Einsatz.Einsatzmittel.Select(faxVehicle => faxVehicle.Id);
                            var resultVehicle = group.Vehicles
                                .Where(groupVehicle => faxVehicleIds.Contains(groupVehicle.Id)).ToList();

                            var pagerIds = _currentPagerList.Select(pager => pager.Id);
                            var resultPager = group.Pagers.Where(groupPager => pagerIds.Contains(groupPager.Id))
                                .ToList();

                            if (resultVehicle != null && resultVehicle.Count >= 1 &&
                                resultPager != null && resultPager.Count >= 1)
                                alarmDto.Groups.Add(group.GroupId);
                        }
                        else // no fahrzeug filter & no pager filter
                        {
                            alarmDto.Groups.Add(group.GroupId);
                        }

                    if (alarmDto.Groups.Count == 0)
                    {
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), "create alarm -> no alarmgroups found");
                        return;
                    }

                    //Create titel        
                    var _titel = "EINSATZ";
                    if (!string.IsNullOrWhiteSpace(e.Einsatz.Stichwort) &&
                        !string.IsNullOrWhiteSpace(e.Einsatz.Schlagwort))
                        _titel = $"{e.Einsatz.Stichwort} - {e.Einsatz.Schlagwort}";
                    else if (!string.IsNullOrWhiteSpace(e.Einsatz.Stichwort) &&
                             string.IsNullOrWhiteSpace(e.Einsatz.Schlagwort))
                        _titel = $"{e.Einsatz.Stichwort}";
                    else if (string.IsNullOrWhiteSpace(e.Einsatz.Stichwort) &&
                             !string.IsNullOrWhiteSpace(e.Einsatz.Schlagwort))
                        _titel = $"{e.Einsatz.Schlagwort}";

                    alarmDto.Alarm.Title = _titel;

                    //Set vehicles
                    alarmDto.Alarm.AlarmDetails.Vehicles = e.Einsatz.Einsatzmittel?.Select(x => x.Name).ToList();
                    alarmDto.Alarm.AlarmDetails.Object = e.Einsatz?.Objekt;

                    //Set coordinates                    
                    if (!string.IsNullOrEmpty(e.Einsatz.Ort)) alarmDto.Alarm.AlarmDetails.Address.City = e.Einsatz.Ort;

                    if (!string.IsNullOrEmpty(e.Einsatz.Straße))
                        alarmDto.Alarm.AlarmDetails.Address.Street = new Street
                        {
                            line = $"{e.Einsatz.Straße} {e.Einsatz.Hausnummer}"
                        };

                    ;
                    var coordinaten = e.Einsatz.KoordinatenWGS84();
                    if (coordinaten != null)
                    {
                        alarmDto.Alarm.AlarmDetails.Address.Coordinates = new Coordinates();
                        alarmDto.Alarm.AlarmDetails.Address.Coordinates.Latitude = coordinaten.Latitude;
                        alarmDto.Alarm.AlarmDetails.Address.Coordinates.Longitude = coordinaten.Longitude;
                    }

                    //Create text  
                    if (!string.IsNullOrEmpty(e.Einsatz.Bemerkung))
                    {
                        alarmDto.Alarm.AlarmDetails.Texts = new List<string>();
                        alarmDto.Alarm.AlarmDetails.Texts.Add(e.Einsatz.Bemerkung);
                    }

                    //Send alarm to backend  
                    Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"create alarm -> {alarmDto}");

                    if (string.IsNullOrEmpty(_currentAlarmId))
                    {
                        var _alarmResult = _apiService.CreateAlarm(alarmDto);
                        if (_alarmResult == null || string.IsNullOrEmpty(_alarmResult.alarm_id))
                        {
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "create alarm -> ERROR");
                            return;
                        }

                        _currentAlarmId = _alarmResult.alarm_id;
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"create alarm -> alarmId[{_currentAlarmId}]");
                    }
                    else
                    {
                        var _alarmResult = _apiService.UpdateAlarm(_currentAlarmId, alarmDto);
                        if (_alarmResult == null || string.IsNullOrEmpty(_alarmResult.alarm_id))
                        {
                            Logger.WriteDebug(MethodBase.GetCurrentMethod(), "update alarm -> ERROR");
                            return;
                        }

                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), $"update alarm -> alarmId[{_currentAlarmId}]");
                    }

                    Alarmed.RaiseEvent(this, new AlarmedEventArgs(e.Einsatz.Guid, _currentAlarmId));
                    timerAlarmEnd_Elapsed(this, null); // after a fax, alarm could not be updated
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