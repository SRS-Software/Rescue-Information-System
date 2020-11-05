#region

using System;
using System.Reflection;
using RestSharp;
using RestSharp.Serializers;
using RIS.Core.Fax;
using RIS.Core.FireboardApi;
using RIS.Core.Helper;
using RIS.Properties;
using SRS.Utilities;

#endregion

namespace RIS.Core
{
    public class FireboardApiService
    {
        private readonly AuthKey _key;

        public FireboardApiService(AuthKey key)
        {
            _key = key;
        }

        public FireboardApiService(string key)
        {
            _key = new AuthKey(key);
        }


        public bool IsAuthTokenValid()
        {
            if (_key == null) throw new ArgumentNullException("AuthKey");

            if (!_key.IsValid) return false;

            return true;
        }

        public void CreateAlarm(Einsatz einsatz)
        {
            if (!IsAuthTokenValid()) return;

            if (einsatz == null) throw new ArgumentException("einsatz");

            var operation = new FireboardOperation();
            operation.Version = "1.0";
            operation.UniqueId = Guid.NewGuid().ToString("N");
            operation.BasicData = new BasicData
            {
                ExternalNumber = "RISv" + Assembly.GetExecutingAssembly().GetName().Version,
                Keyword = einsatz.Stichwort,
                Announcement = einsatz.Schlagwort,
                Location = $"{einsatz.Straße} {einsatz.Hausnummer}",
                Situation = einsatz.Bemerkung
                //_text += "EINSATZMITTEL:" + Environment.NewLine;
                //foreach (Vehicle _vehicle in e.Einsatz.Einsatzmittel)
                //{
                //    _text += _vehicle.Name + Environment.NewLine;
                //}
            };

            if (!string.IsNullOrWhiteSpace(einsatz.KoordinatenRW) && !string.IsNullOrWhiteSpace(einsatz.KoordinatenHW))
            {
                var _coordinaten = new Coordinaten();
                _coordinaten.GaussToWGS84(Convert.ToDouble(einsatz.KoordinatenRW),
                    Convert.ToDouble(einsatz.KoordinatenHW));

                operation.BasicData.Geo_location = new Geo_location
                {
                    Latitude = _coordinaten.Latitude,
                    Longitude = _coordinaten.Longitude
                };
            }

            var client = new RestClient(Settings.Default.Fireboard_WebserviceUrl);
            client.UserAgent = "RISv" + Assembly.GetExecutingAssembly().GetName().Version;
            var request = new RestRequest(@"api?authkey={authkey}&call={calltype}", Method.POST);
            request.AddUrlSegment("authkey", Settings.Default.Fireboard_AuthKey);
            request.AddUrlSegment("calltype", "operation_data");
            request.RequestFormat = DataFormat.Xml;
            request.XmlSerializer = new DotNetXmlSerializer();
            request.AddXmlBody(operation);

            var response = client.Execute(request);
            if (response.ErrorException != null)
                Logger.WriteError(MethodBase.GetCurrentMethod(), "Error retrieving response: " + response.ErrorMessage);
        }
    }
}