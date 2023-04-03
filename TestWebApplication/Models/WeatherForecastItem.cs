using System.ComponentModel.DataAnnotations;

namespace TestWebApplication.Models
{
    public partial class WeatherForecastItem
    {
        [Key]
        public DateOnly Date { get; set; }
        public int TemperatureCNight { get; set; }
        public int TemperatureCMorning { get; set; }

        public int TemperatureCAfternoon { get; set; }
        public int TemperatureCEvening { get; set; }
    }
}
