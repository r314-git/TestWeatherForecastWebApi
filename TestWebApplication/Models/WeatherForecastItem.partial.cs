namespace TestWebApplication.Models
{
    public partial class WeatherForecastItem
    {
        static public WeatherForecastItem DefaultWeatherForecastItem ()
        {
            return new WeatherForecastItem()
            {
                Date = new DateOnly(),
                TemperatureCNight = -1,
                TemperatureCMorning = 0,
                TemperatureCAfternoon = 1,
                TemperatureCEvening = 2
            };
        }
    }
}
