#region

using System;
using System.Collections.Generic;
using System.Globalization;

#endregion

namespace RIS.Core.Map
{
    public class RoutingResult
    {
        public class Rootobject
        {
            public Response response { get; set; }

            public bool IsValid
            {
                get
                {
                    if (response?.route?[0]?.leg[0]?.maneuver?.Count > 0) return true;

                    if (response?.route?[0]?.shape?.Count > 0) return true;

                    return false;
                }
            }

            public List<string> Line => response?.route?[0]?.shape;

            public double? Distance => response?.route?[0]?.summary?.distance;

            public TimeSpan? Duration
            {
                get
                {
                    if (response?.route?[0]?.summary?.travelTime != null)
                        return TimeSpan.FromSeconds(response.route[0].summary.travelTime);

                    return null;
                }
            }

            public string StartpointText => response?.route?[0]?.waypoint[0]?.label;

            public string StartpointCoordinates
            {
                get
                {
                    string _result = null;
                    _result += response?.route?[0]?.waypoint?[0]?.originalPosition?.latitude
                        .ToString(CultureInfo.InvariantCulture) + ",";
                    _result += response?.route?[0]?.waypoint?[0]?.originalPosition?.longitude
                        .ToString(CultureInfo.InvariantCulture);
                    return _result;
                }
            }

            public string StoppointText => response?.route?[0]?.waypoint?[1]?.label;

            public string StoppointCoordinates
            {
                get
                {
                    string _result = null;
                    _result += response?.route?[0]?.waypoint?[1]?.originalPosition?.latitude
                        .ToString(CultureInfo.InvariantCulture) + ",";
                    _result += response?.route?[0]?.waypoint?[1]?.originalPosition?.longitude
                        .ToString(CultureInfo.InvariantCulture);
                    return _result;
                }
            }
        }


        public class Response
        {
            public Metainfo metaInfo { get; set; }
            public List<Route> route { get; set; }
            public string language { get; set; }
        }

        public class Metainfo
        {
            public DateTime timestamp { get; set; }
            public string mapVersion { get; set; }
            public string moduleVersion { get; set; }
            public string interfaceVersion { get; set; }
        }

        public class Route
        {
            public string routeId { get; set; }
            public List<Waypoint> waypoint { get; set; }
            public Mode mode { get; set; }
            public List<string> shape { get; set; }
            public List<Leg> leg { get; set; }
            public Summary summary { get; set; }
        }

        public class Mode
        {
            public string type { get; set; }
            public List<string> transportModes { get; set; }
            public string trafficMode { get; set; }
            public List<object> feature { get; set; }
        }

        public class Summary
        {
            public int distance { get; set; }
            public int trafficTime { get; set; }
            public int baseTime { get; set; }
            public List<string> flags { get; set; }
            public string text { get; set; }
            public int travelTime { get; set; }
            public string _type { get; set; }
        }

        public class Waypoint
        {
            public string linkId { get; set; }
            public Mappedposition mappedPosition { get; set; }
            public Originalposition originalPosition { get; set; }
            public string type { get; set; }
            public float spot { get; set; }
            public string sideOfStreet { get; set; }
            public string mappedRoadName { get; set; }
            public string label { get; set; }
            public int shapeIndex { get; set; }
        }

        public class Mappedposition
        {
            public float latitude { get; set; }
            public float longitude { get; set; }
        }

        public class Originalposition
        {
            public float latitude { get; set; }
            public float longitude { get; set; }
        }

        public class Leg
        {
            public List<Maneuver> maneuver { get; set; }
        }

        public class Maneuver
        {
            public Position position { get; set; }
            public string instruction { get; set; }
            public int travelTime { get; set; }
            public int length { get; set; }
            public List<string> shape { get; set; }
            public List<Note> note { get; set; }
            public string id { get; set; }
            public string _type { get; set; }
        }

        public class Position
        {
            public float latitude { get; set; }
            public float longitude { get; set; }
        }

        public class Note
        {
            public string type { get; set; }
            public string code { get; set; }
            public string text { get; set; }
        }
    }
}