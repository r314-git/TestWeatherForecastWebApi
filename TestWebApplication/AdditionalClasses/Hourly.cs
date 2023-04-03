using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenMeteo
{
    //open-meteo return json object, one of the field is object of type `Hourly`
    //this class is only for correctness of json parsing
    public class Hourly
    {
        [JsonPropertyName("time")]
        public string[]? Time { get; set; }
        [JsonPropertyName("temperature_2m")]
        public float?[]? Temperature_2m { get; set; }
    }
}