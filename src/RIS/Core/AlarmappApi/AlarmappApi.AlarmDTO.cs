#region

using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#endregion

namespace RIS.Core.AlarmappApi
{
    public partial class AlarmDto
    {
        [JsonProperty("groups")] public List<string> Groups { get; set; } = new List<string>();

        [JsonProperty("alarm")] public Alarm Alarm { get; set; } = new Alarm();
    }

    public class Alarm
    {
        [JsonProperty("title")] public string Title { get; set; }

        [JsonProperty("alarm_details")] public AlarmDetails AlarmDetails { get; set; } = new AlarmDetails();
    }

    public class AlarmDetails
    {
        [JsonProperty("category")] public string Category { get; set; }

        [JsonProperty("texts")] public List<string> Texts { get; set; }

        [JsonProperty("cars")] public List<string> Vehicles { get; set; }

        [JsonProperty("object")] public string Object { get; set; }

        [JsonProperty("address")] public Address Address { get; set; } = new Address();

        [JsonProperty("raw")] public string Raw { get; set; }
    }

    public class Address
    {
        [JsonProperty("city")] public string City { get; set; }

        [JsonProperty("city_code")] public string City_code { get; set; }

        [JsonProperty("district")] public string District { get; set; }

        [JsonProperty("simple")] public Street Street { get; set; }

        [JsonProperty("coordinates")] public Coordinates Coordinates { get; set; }

        [JsonProperty("raw")] public string Raw { get; set; }
    }

    public class Street
    {
        [JsonProperty("line1")] public string line { get; set; }
    }

    public class Coordinates
    {
        [JsonProperty("lat")] public double Latitude { get; set; }

        [JsonProperty("lon")] public double Longitude { get; set; }
    }

    public partial class AlarmDto
    {
        public static AlarmDto FromJson(string json)
        {
            return JsonConvert.DeserializeObject<AlarmDto>(json, Converter.Settings);
        }
    }

    public static class Serialize
    {
        public static string ToJson(this AlarmDto self)
        {
            return JsonConvert.SerializeObject(self, Converter.Settings);
        }
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter {DateTimeStyles = DateTimeStyles.AssumeUniversal}
            }
        };
    }
}