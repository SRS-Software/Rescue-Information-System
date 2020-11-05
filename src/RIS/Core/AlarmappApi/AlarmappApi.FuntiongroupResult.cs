#region

using Newtonsoft.Json;

#endregion

namespace RIS.Core.AlarmappApi
{
    public class Functiongroup
    {
        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("colors")] public Colors Colors { get; set; }

        [JsonProperty("short_key")] public string ShortKey { get; set; }

        [JsonProperty("priority")] public long Priority { get; set; }
    }

    public class Colors
    {
        [JsonProperty("background")] public string Background { get; set; }

        [JsonProperty("foreground")] public string Foreground { get; set; }
    }
}