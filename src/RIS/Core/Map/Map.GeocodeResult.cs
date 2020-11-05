#region

using System;
using System.Collections.Generic;
using System.Globalization;

#endregion

namespace RIS.Core.Map
{
    public class GeocodeResult
    {
        public class Rootobject
        {
            public Response Response { get; set; }

            public bool IsValid
            {
                get
                {
                    if (Response == null) return false;

                    if (Response.View == null || Response.View.Count <= 0) return false;

                    if (Response.View[0].Result == null || Response.View[0].Result.Count <= 0) return false;

                    if (Response.View[0].Result[0].Location == null) return false;

                    if (Response.View[0].Result[0].Location.NavigationPosition == null ||
                        Response.View[0].Result[0].Location.NavigationPosition.Count <= 0)
                        return false;

                    return true;
                }
            }

            public string Location
            {
                get
                {
                    if (!IsValid) return null;

                    var _result = string.Empty;
                    _result += Response.View[0].Result[0].Location.NavigationPosition[0].Latitude
                        .ToString(CultureInfo.InvariantCulture) + ",";
                    _result += Response.View[0].Result[0].Location.NavigationPosition[0].Longitude
                        .ToString(CultureInfo.InvariantCulture);
                    return _result;
                }
            }

            public float Quality
            {
                get
                {
                    if (!IsValid) return 0;

                    return Response.View[0].Result[0].MatchQuality.HouseNumber;
                }
            }
        }

        public class Response
        {
            public Metainfo MetaInfo { get; set; }
            public List<View> View { get; set; }
        }

        public class Metainfo
        {
            public DateTime Timestamp { get; set; }
        }

        public class View
        {
            public string _type { get; set; }
            public int ViewId { get; set; }
            public List<Result> Result { get; set; }
        }

        public class Result
        {
            public float Relevance { get; set; }
            public string MatchLevel { get; set; }
            public Matchquality MatchQuality { get; set; }
            public string MatchType { get; set; }
            public Location Location { get; set; }
        }

        public class Matchquality
        {
            public float District { get; set; }
            public List<float> Street { get; set; }
            public float HouseNumber { get; set; }
            public float PostalCode { get; set; }
        }

        public class Location
        {
            public string LocationId { get; set; }
            public string LocationType { get; set; }
            public Displayposition DisplayPosition { get; set; }
            public List<Navigationposition> NavigationPosition { get; set; }
            public Mapview MapView { get; set; }
            public Address Address { get; set; }
        }

        public class Displayposition
        {
            public float Latitude { get; set; }
            public float Longitude { get; set; }
        }

        public class Mapview
        {
            public Topleft TopLeft { get; set; }
            public Bottomright BottomRight { get; set; }
        }

        public class Topleft
        {
            public float Latitude { get; set; }
            public float Longitude { get; set; }
        }

        public class Bottomright
        {
            public float Latitude { get; set; }
            public float Longitude { get; set; }
        }

        public class Address
        {
            public string Label { get; set; }
            public string Country { get; set; }
            public string State { get; set; }
            public string County { get; set; }
            public string City { get; set; }
            public string District { get; set; }
            public string Street { get; set; }
            public string HouseNumber { get; set; }
            public string PostalCode { get; set; }
            public List<Additionaldata> AdditionalData { get; set; }
        }

        public class Additionaldata
        {
            public string value { get; set; }
            public string key { get; set; }
        }

        public class Navigationposition
        {
            public float Latitude { get; set; }
            public float Longitude { get; set; }
        }
    }
}