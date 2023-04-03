using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TestWebApplication.Models;

namespace TestWebApplication.Data
{
    public class WeatherForecastContext : DbContext
    {
        public DbSet<WeatherForecastItem> WeatherForecastItems { get; set; } = null!;

        public WeatherForecastContext(DbContextOptions<WeatherForecastContext> options)
        : base(options)
        {
        }
    }
}
