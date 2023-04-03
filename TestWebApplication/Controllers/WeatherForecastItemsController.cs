using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestWebApplication.AdditionalClasses;
using TestWebApplication.Data;
using TestWebApplication.Models;


namespace TestWebApplication.Controllers
{
    [Route("api")]
    [ApiController]
    public class WeatherForecastItemsController : ControllerBase
    {
        private readonly WeatherForecastContext _context;
        private readonly ILogger<WeatherForecastItemsController> _logger;
        private readonly IExternalWeatherForecast _externalWeatherForecast;
        public WeatherForecastItemsController(WeatherForecastContext context, ILogger<WeatherForecastItemsController> logger, IExternalWeatherForecast externalWeatherForecast)
        {
            _context = context;
            _logger = logger;
            _externalWeatherForecast = externalWeatherForecast;
        }

        // return 14 days forecast
        // GET: api/temperature
        [HttpGet("temperature")]
        public async Task<IEnumerable<WeatherForecastItem>> GetWeatherForecast()
        {
            int NumberOfDays = 14;
            DateOnly start = DateOnly.FromDateTime(DateTime.Now);
            DateOnly end = start.AddDays(NumberOfDays - 1);

            var res = await _context.WeatherForecastItems.Select(x => x.Date >= start && x.Date <= end).ToListAsync();
            if (res.Count < 14)
            {
                List<WeatherForecastItem> forecast = await _externalWeatherForecast.GetWeatherForecastAsync(start, end);
                foreach (var item in forecast)
                {
                    if (!WeatherForecastItemExists(item.Date))
                    {
                        _context.WeatherForecastItems.Add(item);
                    }
                }
                await _context.SaveChangesAsync();
            }
            return await _context.WeatherForecastItems.Where(x => x.Date >= start && x.Date <= end).OrderBy(x => x.Date).ToListAsync();
        }

        // uodate forecast in db, period not more 16 days
        // {"fromDate": "2023-03-31","toDate": "2023-04-3"}
        // POST: api/temperature/importdata
        [HttpPost("temperature/importdata")]
        public async Task<IResult> PostWeatherForecastItem(string period)
        {
            Period? tmp = null;

            try
            {
                tmp = JsonSerializer.Deserialize<Period>(period);
            }
            catch (Exception) { 
            }

            if (tmp == null)
                return Results.BadRequest("incorrect format of json");

            DateOnly start, end;
            bool okStart = DateOnly.TryParse(tmp.TimeStart, out start);
            bool okEnd = DateOnly.TryParse(tmp.TimeEnd, out end);
            if (!okStart || !okEnd)
            {
                return Results.BadRequest("incorrect period: should contain `fromDate` and `toDate` fields, each in format'yyyy-mm-dd'");
            }
            int numberOfDays = IExternalWeatherForecast.NumberOfDays(start, end);
            if (numberOfDays > 16 || numberOfDays < 0)
            {
                return Results.BadRequest("Number of days in period allowed to be in range 0 to 16.");
            }

            List<WeatherForecastItem> forecast;
            try
            {
                forecast = await _externalWeatherForecast.GetWeatherForecastAsync(start, end);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }

            if (_context.WeatherForecastItems == null)
                return Results.StatusCode(StatusCodes.Status500InternalServerError);

            foreach (var item in forecast)
            {  
                WeatherForecastItem? existingRecord = _context.WeatherForecastItems.FirstOrDefault(x => x.Date == item.Date);

                if (existingRecord != null)
                    _context.WeatherForecastItems.Remove(existingRecord);
              
                _context.WeatherForecastItems.Add(item);
            }
 
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex.ToString());
                return Results.StatusCode(StatusCodes.Status500InternalServerError); 
            }

            return Results.Ok();
        }

        private bool WeatherForecastItemExists(DateOnly id)
        {
            return (_context.WeatherForecastItems?.Any(e => e.Date == id)).GetValueOrDefault();
        }
    }
}
