using System.Text.Json.Serialization;

namespace OpenMeteo
{
    // Weather Forecast API response
    // This class is only for correctness of json parsing
    public class WeatherForecast
    {
        public float Latitude { get; set; }

        public float Longitude { get; set; }

        [JsonPropertyName("generationtime_ms")]
        public float GenerationTime { get; set; }

        [JsonPropertyName("utc_offset_seconds")]
        public int UtcOffset { get; set; }

        [JsonPropertyName("timezone")]
        public string? Timezone { get; set; }

        [JsonPropertyName("timezone_abbreviation")]
        public string? TimezoneAbbreviation { get; set; }

        [JsonPropertyName("hourly")]
        public Hourly? Hourly { get; set; }
    }
}

