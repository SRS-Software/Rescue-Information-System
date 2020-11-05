#region

using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using RestSharp;
using RIS.Core.Fax;
using RIS.Core.Map;
using RIS.Properties;
using SRS.Utilities;
using SRS.Utilities.Extensions;

#endregion

namespace RIS.Core
{
    public class MapService : IMapService
    {
        private static readonly string here_AppId = HttpUtility.UrlEncode("ZQedbuK7qlrnbcErPgOn");
        private static readonly string here_AppCode = HttpUtility.UrlEncode("ED05Y9gE9ogFWNrH4edpgg");

        private readonly IFaxService _faxService;

        public MapService(IFaxService faxService)
        {
            try
            {
                _faxService = faxService;
                _faxService.EinsatzCreated += faxService_EinsatzCreated;

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

        public string Geocode(string _address)
        {
            try
            {
                return clientGeocode(_address).ToString();
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
        public event EventHandler<FinishedEventArgs> Finished;

        #endregion //Events

        #region Private Properties

        #endregion //Private Properties

        #region Private Funtions

        private void faxService_EinsatzCreated(object sender, EinsatzCreatedEventArgs e)
        {
            if (e == null || e.Einsatz == null) return;

            Task.Factory.StartNew(() =>
            {
                var finishedEventArgs = new FinishedEventArgs(e.Einsatz);

                if (IsRunning == false)
                {
                    //To start PrinterService
                    Finished.RaiseEvent(this, finishedEventArgs);
                    return;
                }

                try
                {
                    //start location    
                    var _startLocation = new Location(Settings.Default.Route_StartLocation);
                    if (_startLocation == null)
                    {
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), "no startpoint");
                        return;
                    }

                    //stop location
                    Location _stopLocation = null;
                    var coordinaten = e.Einsatz.KoordinatenWGS84();
                    if (coordinaten != null) _stopLocation = new Location(coordinaten.Latitude, coordinaten.Longitude);

                    if (_stopLocation == null)
                    {
                        var _stopAddress = $"{e.Einsatz.Ort}, {e.Einsatz.Straße} {e.Einsatz.Hausnummer}";
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), "set address -> " + _stopAddress);

                        _stopLocation = clientGeocode(_stopAddress);
                    }

                    if (_stopLocation == null)
                    {
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), "destination not found");
                        return;
                    }
                    else
                    {
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), "destination found -> " + _stopLocation);
                    }

                    var _route = clientRouting(_startLocation, _stopLocation);
                    if (_route == null || !_route.IsValid)
                    {
                        Logger.WriteDebug(MethodBase.GetCurrentMethod(), "route not found");
                        return;
                    }

                    finishedEventArgs.Found = true;
                    finishedEventArgs.Distance = _route.Distance;
                    finishedEventArgs.ImageWindow = clientMapImage(_route, Settings.Default.Route_ImageSize.X,
                        Settings.Default.Route_ImageSize.Y, e.Einsatz.Marker);
                    finishedEventArgs.ImageReport = clientMapImage(_route, 1024, 647, e.Einsatz.Marker);
                    finishedEventArgs.Description = clientGetDescription(_route);
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
                    //To start PrinterService
                    Finished.RaiseEvent(this, finishedEventArgs);
                }
            });
        }

        private Location clientGeocode(string _address)
        {
            if (string.IsNullOrWhiteSpace(_address)) throw new ArgumentNullException(nameof(_address));

            var _client = new RestClient("http://geocoder.api.here.com");
            _client.UserAgent = "RISv" + Assembly.GetExecutingAssembly().GetName().Version;

            var _request = new RestRequest(Method.GET);
            _request.RequestFormat = DataFormat.Json;
            _request.Resource = @"/6.2/geocode.json";
            _request.AddParameter("app_id", here_AppId);
            _request.AddParameter("app_code", here_AppCode);
            _request.AddParameter("country", "Germany");
            _request.AddParameter("searchtext", _address);
            _request.AddParameter("gen", "8");
            _request.AddParameter("maxresults", "1");

            var _response = _client.Execute<GeocodeResult.Rootobject>(_request);
            if (_response.ErrorException != null)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(),
                    "Error retrieving response: " + _response.ErrorMessage);
                return null;
            }

            if (_response.Data == null || !_response.Data.IsValid)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), "Error retrieving response: No Data");
                return null;
            }

            if (Settings.Default.Route_HighQuality && _response.Data.Quality <= 0.95)
            {
                Logger.WriteDebug(MethodBase.GetCurrentMethod(),
                    "Location not high quality -> " + _response.Data.Quality);
                return null;
            }

            return new Location(_response.Data.Location);
        }

        private RoutingResult.Rootobject clientRouting(Location _startLocation, Location _stopLocation)
        {
            if (_startLocation == null) throw new ArgumentNullException(nameof(_startLocation));

            if (_stopLocation == null) throw new ArgumentNullException(nameof(_stopLocation));

            //Create mode string
            var _routeMode = string.Empty;
            _routeMode +=
                Settings.Default.Route_ModeFastest || Settings.Default.Route_ModeTruck
                    ? "fastest;"
                    : "shortest;";
            _routeMode += Settings.Default.Route_ModeTruck ? "truck;" : "car;";
            _routeMode += Settings.Default.Route_ModeTraffic ? "traffic:enabled" : "traffic:disabled";

            var _client = new RestClient("http://route.api.here.com");
            _client.UserAgent = "RISv" + Assembly.GetExecutingAssembly().GetName().Version;

            var _request = new RestRequest(Method.GET);
            _request.RequestFormat = DataFormat.Json;
            _request.Resource = @"/routing/7.2/calculateroute.json";
            _request.AddParameter("app_id", here_AppId);
            _request.AddParameter("app_code", here_AppCode);
            _request.AddParameter("waypoint0", _startLocation);
            _request.AddParameter("waypoint1", _stopLocation);
            _request.AddParameter("mode", _routeMode);
            _request.AddParameter("RouteAttributes", "routeId");

            var _response = _client.Execute<RoutingResult.Rootobject>(_request);
            if (_response.ErrorException != null)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(),
                    "Error retrieving response: " + _response.ErrorMessage);
                return null;
            }

            if (_response.Data == null || !_response.Data.IsValid)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), "Error retrieving response: No Data");
                return null;
            }

            _request = new RestRequest(Method.GET);
            _request.RequestFormat = DataFormat.Json;
            _request.Resource = @"/routing/7.2/getroute.json";
            _request.AddParameter("app_id", here_AppId);
            _request.AddParameter("app_code", here_AppCode);
            _request.AddParameter("waypoint0", _startLocation);
            _request.AddParameter("waypoint1", _stopLocation);
            _request.AddParameter("mode", _routeMode);
            _request.AddParameter("routeAttributes", "routeId,summary");
            _request.AddParameter("routeid", _response.Data.response.route[0].routeId);
            _request.AddParameter("language", "de-de");
            _request.AddParameter("representation", "display");
            _request.AddParameter("alternatives", "0");
            _request.AddParameter("avoidseasonalclosures", true);

            _response = _client.Execute<RoutingResult.Rootobject>(_request);
            if (_response.ErrorException != null)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(),
                    "Error retrieving response: " + _response.ErrorMessage);
                return null;
            }

            if (_response.Data == null || !_response.Data.IsValid)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), "Error retrieving response: No Data");
                return null;
            }

            return _response.Data;
        }

        private byte[] clientMapImage(RoutingResult.Rootobject _route, double _width, double _height, int _marker)
        {
            if (_route == null) throw new ArgumentNullException(nameof(_route));

            if (!_route.IsValid) throw new ArgumentException("_route -> not valid");

            if (_width <= 0) throw new ArgumentOutOfRangeException(nameof(_width));

            if (_height <= 0) throw new ArgumentOutOfRangeException(nameof(_height));

            //Scale the size to be not larger than max values
            var _maxImageWidth = 2048;
            var _maxImageHeight = 2048;
            if (_width > _maxImageWidth)
            {
                var _widthScale = _maxImageWidth / _width;
                _width *= _widthScale;
                _height *= _widthScale;
            }

            if (_height > _maxImageHeight)
            {
                var _heightScale = _maxImageHeight / _height;
                _width *= _heightScale;
                _height *= _heightScale;
            }

            var _client = new RestClient("https://image.maps.api.here.com/");
            _client.UserAgent = "RISv" + Assembly.GetExecutingAssembly().GetName().Version;

            var _request = new RestRequest(Method.GET);
            _request.RequestFormat = DataFormat.Json;
            _request.Resource = @"/mia/1.6/route";
            _request.AddParameter("app_id", here_AppId);
            _request.AddParameter("app_code", here_AppCode);
            _request.AddParameter("f", 0); //format: png   
            _request.AddParameter("h", _height); //Image height    
            _request.AddParameter("w", _width); //Image height   
            _request.AddParameter("ml", "ger"); //language       
            _request.AddParameter("t", "12"); //Map scheme type 
            var _routeLine = _route.Line; //Set routing points and must limit to 8k
            var _routeLineIndex = 1;
            while (Encoding.ASCII.GetByteCount(string.Join(",", _routeLine)) > 6000 &&
                   _routeLineIndex < _routeLine.Count)
            {
                _routeLine.RemoveAt(_routeLineIndex);
                _routeLineIndex++;
            }

            _request.AddParameter("r", string.Join(",", _routeLine)); //line points  
            _request.AddParameter("lc", "A70000"); //line color
            _request.AddParameter("lw", "10"); //line width     
            _request.AddParameter("poix0",
                $"{_route.StartpointCoordinates};{"C00000FF"};{""};{"18"};{"Start"}"); //line width     
            _request.AddParameter("poix1",
                $"{_route.StoppointCoordinates};{"C0A70000"};{""};{"18"};{"Einsatz"}"); //line width   
            if (_route.Distance > 10000 && _route.Distance < 30000)
            {
                _request.AddParameter("z", _route.Distance > 20000 ? "14" : "15"); //zoom (0-20)                   
                _request.AddParameter("ctr", _route.StoppointCoordinates); //center point       
            }

            var _response = _client.Execute(_request);
            if (_response.ErrorException != null)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(),
                    "Error retrieving response: " + _response.ErrorMessage);
                return null;
            }

            if (_response.RawBytes == null || _response.RawBytes.Length <= 500)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), "Error retrieving response: No Data");
                return null;
            }

            return _response.RawBytes;
        }

        private string clientGetDescription(RoutingResult.Rootobject _route)
        {
            if (_route == null) throw new ArgumentNullException(nameof(_route));

            if (!_route.IsValid) throw new ArgumentException("_route -> not valid");

            var _htmlDescription = string.Empty;
            _htmlDescription += @"<html>";
            _htmlDescription += @"<body>";
            _htmlDescription += @"<font size=2>";
            _htmlDescription += @"<p>";
            _htmlDescription += $@"<b>Ziel: </b>{_route.StoppointText}<br />";
            if (_route.Duration != null)
                _htmlDescription += $@"<b>Dauer: </b>{_route.Duration.Value.TotalMinutes.ToString("F1")} min<br />";

            if (_route.Distance != null)
                _htmlDescription += $@"<b>Strecke: </b>{(_route.Distance.Value / 1000).ToString("F1")} km<br />";

            _htmlDescription += @"</p>";
            _htmlDescription += @"</font>";
            _htmlDescription += @"<hr />";

            // Write out the instructions 
            var _maneuverIndex = 1;
            foreach (var _maneuver in _route.response.route[0].leg[0].maneuver)
            {
                _htmlDescription += @"<p>";
                _htmlDescription +=
                    $@"<b>{_maneuverIndex}.</b> {_maneuver.instruction} [{TimeSpan.FromSeconds(_maneuver.travelTime).TotalMinutes.ToString("F1")} min]<br />";
                _htmlDescription += @"</p>";
                _maneuverIndex++;
            }

            // Close all open html tags
            _htmlDescription += @"</font>";
            _htmlDescription += @"</body>";
            _htmlDescription += @"</html>";

            return _htmlDescription;
        }

        #endregion //Private Funtions
    }
}