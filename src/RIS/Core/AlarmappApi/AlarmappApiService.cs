#region

using System;
using System.Collections.Generic;
using System.Reflection;
using RestSharp;
using RIS.Core.AlarmappApi;
using RIS.Properties;
using SRS.Utilities;

#endregion

namespace RIS.Core
{
    public class AlarmappApiService
    {
        private readonly string _token;

        public AlarmappApiService(string token)
        {
            _token = token;
        }

        public List<AlarmgroupsResult> GetAlarmgroups(string organisationId)
        {
            if (_token == null) throw new ArgumentNullException("token");

            if (string.IsNullOrEmpty(organisationId)) throw new ArgumentNullException(nameof(organisationId));

            var _request = new RestRequest(Method.GET)
            {
                RequestFormat = DataFormat.Json,
                Resource = $"organisation/{organisationId}/alarmgroup"
            };
            _request.AddHeader("Content-Type", "application/json");
            _request.AddHeader("Authorization", $"Bearer {_token}");

            var _client = new RestClient(Settings.Default.Alarmapp_WebserviceUrl2);
            _client.UserAgent = "RISv" + Assembly.GetExecutingAssembly().GetName().Version;

            var _response = _client.Execute<List<AlarmgroupsResult>>(_request);
            if (!_response.IsSuccessful)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(),
                    "response not successful -> " + _response.StatusDescription);
                return null;
            }

            if (_response.ErrorException != null)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(),
                    "Error retrieving response -> " + _response.ErrorMessage);
                return null;
            }

            if (_response.Data == null)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), "Error retrieving response -> No Data");
                return null;
            }

            return _response.Data;
        }

        public List<Functiongroup> GetFunctiongroups(string organisationId)
        {
            if (_token == null) throw new ArgumentNullException("token");

            if (string.IsNullOrEmpty(organisationId)) throw new ArgumentNullException(nameof(organisationId));

            var _request = new RestRequest(Method.GET)
            {
                RequestFormat = DataFormat.Json,
                Resource = $"organisation/{organisationId}/functiongroup"
            };
            _request.AddHeader("Content-Type", "application/json");
            _request.AddHeader("Authorization", $"Bearer {_token}");

            var _client = new RestClient(Settings.Default.Alarmapp_WebserviceUrl2);
            _client.UserAgent = "RISv" + Assembly.GetExecutingAssembly().GetName().Version;

            var _response = _client.Execute<List<Functiongroup>>(_request);
            if (_response.ErrorException != null)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(),
                    "Error retrieving response: " + _response.ErrorMessage);
                return null;
            }

            if (_response.Data == null)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), "Error retrieving response: No Data");
                return null;
            }

            return _response.Data;
        }

        public AlarmCreateResult CreateAlarm(AlarmDto alarmDto)
        {
            if (string.IsNullOrEmpty(_token)) throw new ArgumentNullException(nameof(_token));

            if (alarmDto == null) throw new ArgumentNullException("alarmDto");

            var _request = new RestRequest(Method.POST)
            {
                RequestFormat = DataFormat.Json,
                Resource = @"alarm"
            };
            _request.AddHeader("Content-Type", "application/json");
            _request.AddHeader("Authorization", $"Bearer {_token}");

            var test = alarmDto.ToJson();
            _request.AddJsonBody(test);

            var _client = new RestClient(Settings.Default.Alarmapp_WebserviceUrl2);
            _client.UserAgent = "RISv" + Assembly.GetExecutingAssembly().GetName().Version;

            var _response = _client.Execute<AlarmCreateResult>(_request);
            if (_response.ErrorException != null)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(),
                    "Error retrieving response: " + _response.ErrorMessage);
                return null;
            }

            if (_response.Data == null)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), "Error retrieving response: No Data");
                return null;
            }

            return _response.Data;
        }

        public AlarmCreateResult UpdateAlarm(string alarmId, AlarmDto alarmDto)
        {
            if (string.IsNullOrEmpty(_token)) throw new ArgumentNullException(nameof(_token));

            if (string.IsNullOrEmpty(alarmId)) throw new ArgumentNullException(nameof(alarmId));

            if (alarmDto == null) throw new ArgumentNullException("alarmDto");

            var _request = new RestRequest(Method.PUT)
            {
                RequestFormat = DataFormat.Json,
                Resource = $"alarm/{alarmId}"
            };
            _request.AddHeader("Content-Type", "application/json");
            _request.AddHeader("Authorization", $"Bearer {_token}");

            var test = alarmDto.ToJson();
            _request.AddJsonBody(test);

            var _client = new RestClient(Settings.Default.Alarmapp_WebserviceUrl2);
            _client.UserAgent = "RISv" + Assembly.GetExecutingAssembly().GetName().Version;

            var _response = _client.Execute<AlarmCreateResult>(_request);
            if (_response.ErrorException != null)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(),
                    "Error retrieving response: " + _response.ErrorMessage);
                return null;
            }

            if (_response.Data == null)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), "Error retrieving response: No Data");
                return null;
            }

            return _response.Data;
        }

        public List<Participation> GetAlarmParticipations(string operationId)
        {
            if (string.IsNullOrEmpty(_token)) throw new ArgumentNullException(nameof(_token));

            if (string.IsNullOrEmpty(operationId)) throw new ArgumentNullException(nameof(operationId));

            var _request = new RestRequest(Method.GET)
            {
                RequestFormat = DataFormat.Json,
                Resource = $"alarm/{operationId}/participations"
            };
            _request.AddHeader("Content-Type", "application/json");
            _request.AddHeader("Authorization", $"Bearer {_token}");

            var _client = new RestClient(Settings.Default.Alarmapp_WebserviceUrl2);
            _client.UserAgent = "RISv" + Assembly.GetExecutingAssembly().GetName().Version;

            var _response = _client.Execute<List<Participation>>(_request);
            if (_response.ErrorException != null)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(),
                    "Error retrieving response: " + _response.ErrorMessage);
                return null;
            }

            if (_response.Data == null)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), "Error retrieving response: No Data");
                return null;
            }

            return _response.Data;
        }
    }
}