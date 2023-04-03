using System.Text.Json.Serialization;

namespace TestWebApplication.AdditionalClasses
{
    public class Period
    {
        [JsonPropertyName("fromDate")]
        public string TimeStart { get; set; }
        [JsonPropertyName("toDate")]
        public string TimeEnd { get; set; }

        public Period() {
            TimeStart = "0";
            TimeEnd = "0";
        }
    }
}
