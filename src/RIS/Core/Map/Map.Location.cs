#region

using System.Globalization;
using System.Linq;

#endregion

namespace RIS.Core.Map
{
    public class Location
    {
        public Location()
        {
        }

        public Location(double _latitude, double _longitude)
        {
            Latitude = _latitude;
            Longitude = _longitude;
        }

        public Location(string _latitude, string _longitude)
        {
            if (string.IsNullOrWhiteSpace(_latitude)) return;

            if (string.IsNullOrWhiteSpace(_longitude)) return;

            Latitude = double.Parse(_latitude, CultureInfo.InvariantCulture);
            Longitude = double.Parse(_longitude, CultureInfo.InvariantCulture);
        }

        public Location(string _text)
        {
            if (string.IsNullOrWhiteSpace(_text)) return;

            var _coordinates = _text.Split(',');
            if (_coordinates == null || _coordinates.Count() != 2) return;

            Latitude = double.Parse(_coordinates[0], CultureInfo.InvariantCulture);
            Longitude = double.Parse(_coordinates[1], CultureInfo.InvariantCulture);
        }


        public double Latitude { get; set; }
        public double Longitude { get; set; }


        public override string ToString()
        {
            if (Latitude <= 0 || Longitude <= 0) return null;

            return
                $"{Latitude.ToString(CultureInfo.InvariantCulture)},{Longitude.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}