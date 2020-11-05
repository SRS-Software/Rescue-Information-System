#region

using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace RIS.Core.AlarmappApi
{
    public class Participation
    {
        [JsonProperty("user")] public User User { get; set; }

        //[JsonProperty("function_groups")]
        public IList<string> function_groups { get; set; }

        [JsonProperty("updated_at")] public Time UpdatedAt { get; set; }

        [JsonProperty("created_at")] public Time CreatedAt { get; set; }

        [JsonProperty("status")] public int Status { get; set; }

        [JsonProperty("user_id")] public string UserId { get; set; }

        [JsonProperty("id")] public string Id { get; set; }
    }

    public class Time
    {
        [JsonProperty("_seconds")] public long Seconds { get; set; }

        [JsonProperty("_nanoseconds")] public long Nanoseconds { get; set; }
    }

    public class User
    {
        [JsonProperty("name")] public Name Name { get; set; }

        public override string ToString()
        {
            return $"{Name.Last} {Name.First}";
        }
    }

    public class Name
    {
        [JsonProperty("first")] public string First { get; set; }

        [JsonProperty("last")] public string Last { get; set; }
    }
}