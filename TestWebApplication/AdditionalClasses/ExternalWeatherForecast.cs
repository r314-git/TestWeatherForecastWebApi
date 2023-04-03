using System.Text.Json;
using TestWebApplication.Models;

namespace TestWebApplication.AdditionalClasses
{
    /// <summary>
    /// interface for getting weather forecast from external source(open-meteo, gismeteo, etc.)
    /// </summary>
    public interface IExternalWeatherForecast
    {
        public static int NumberOfDays(DateOnly start, DateOnly end)
        {
           return (end.ToDateTime(new TimeOnly(0)) - start.ToDateTime(new TimeOnly(0))).Days + 1;
        }
        /// <summary>
        /// Get weather forecast for period from `start` to `end` (inclusively)
        /// </summary>
        /// <returns> list of `WeatherForecastItem` </returns>
        virtual async Task<List<WeatherForecastItem>> GetWeatherForecastAsync(DateOnly start, DateOnly end)
        {   
            int numberOfDays = NumberOfDays(start, end);
            return Enumerable.Repeat(WeatherForecastItem.DefaultWeatherForecastItem(), numberOfDays).ToList();
        }
    }

    public class ExternalWeatherForecastOpenMeteo : IExternalWeatherForecast
    {
        private OpenMeteo.WeatherForecast? WeatherForecast { get; set; }
        private DateOnly start;
        private DateOnly end;
        private int numberOfDays;

        private int NumberOfDays(DateOnly start, DateOnly end)
        {
            return (end.ToDateTime(new TimeOnly(0)) - start.ToDateTime(new TimeOnly(0))).Days + 1;
        }


        // getting weather forecast from open-meteo, using api `open-meteo`, assuming: 
        // city for forecast is Petrozavodsk (latitude=61.78 & longitude=34.35)
        // 2m from ground (hourly=temperature_2m)
        private async Task<string> getWeatherForecastOpenMeteoJson()
        {
            string startLine = start.ToDateTime(new TimeOnly(0)).ToString("yyyy-MM-dd");
            string endLine = end.ToDateTime(new TimeOnly(0)).ToString("yyyy-MM-dd");

            string urlOpenMeteo = $"https://api.open-meteo.com/v1/forecast?latitude=61.78&longitude=34.35&hourly=temperature_2m&forecast_days={numberOfDays}&start_date={startLine}&end_date={endLine}&timezone=Europe%2FMoscow";
            using (var httpClient = new HttpClient())
            {
                
                var json = await httpClient.GetStringAsync(urlOpenMeteo);
                return json;
            }
        }

        public ExternalWeatherForecastOpenMeteo()
        {
            this.start = new DateOnly();
            this.end = new DateOnly();
            this.numberOfDays = NumberOfDays(start, end);
        }

        private async Task GetWeatherForecastFromOpenMeteo()
        {
            var json = await getWeatherForecastOpenMeteoJson();
            WeatherForecast = JsonSerializer.Deserialize<OpenMeteo.WeatherForecast>(json);
            if (WeatherForecast == null)
                throw new Exception("Open Meteo Error");
        }

        // get forecast from open-meteo and transform data to list of `WeatherForecastItem` elements
        public async Task<List<WeatherForecastItem>> GetWeatherForecastAsync(DateOnly start, DateOnly end)
        {
            this.start = start;
            this.end = end;
            this.numberOfDays = NumberOfDays(start, end);

            await GetWeatherForecastFromOpenMeteo();
            var forecast = new List<WeatherForecastItem>();
            forecast = Enumerable.Range(0, numberOfDays).Select(Index => { return GetWeatherForecastItem(Index); }).ToList();
            return forecast;
        }

        // generate one item of `WeatherForecastItem` by processing information in `WeatherForecast.Hourly.Temperature_2m` array
        // `WeatherForecast.Hourly.Temperature_2m` consists of 24 * numberofdays elements
        // night temperature is average temperature from 0 to 5 hour in day
        // morning temperature is average temperature from 6 to 11 hour in day
        // afternoon temperature is average temperature from 12 to 17 hour in day
        // evening temperature is average temperature from 18 to 23 hour in day
        private WeatherForecastItem GetWeatherForecastItem(int day)
        {
            int firstId = day * 24;

            Func<int, int> getAverageTemperature6ValuesByIndexStart = x =>
            {
                return (int)Math.Round((float)Enumerable.Range(firstId + x, 6).Average(index => WeatherForecast.Hourly.Temperature_2m[index]));
            };

            int nightTemperature = getAverageTemperature6ValuesByIndexStart(0);
            int morningTemperature = getAverageTemperature6ValuesByIndexStart(6);
            int afternoonTemperature = getAverageTemperature6ValuesByIndexStart(12); 
            int eveningTemperature = getAverageTemperature6ValuesByIndexStart(18); 
            

            WeatherForecastItem item = new()
            {
                Date = DateOnly.Parse(WeatherForecast.Hourly.Time[firstId]),
                TemperatureCNight = nightTemperature,
                TemperatureCMorning = morningTemperature,
                TemperatureCAfternoon = afternoonTemperature,
                TemperatureCEvening = eveningTemperature
            };
            return item;
           
        }
    }
}