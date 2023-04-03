using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using TestWebApplication.AdditionalClasses;
using TestWebApplication.Controllers;
using TestWebApplication.Data;
using TestWebApplication.Models;

namespace Tests
{
    public class UnitTest
    {

        private readonly DbContextOptions<WeatherForecastContext> dbContextOptions;
        private List<WeatherForecastItem> testWeatherForecastItemsFromOpenMeteo;

        private List<WeatherForecastItem> dbInitialTestData;
        private List<WeatherForecastItem> dbTestDataAfterQuery;

        private string period;
        private DateOnly start;
        private DateOnly end;

        public UnitTest()
        {
            // Build DbContextOptions
            dbContextOptions = new DbContextOptionsBuilder<WeatherForecastContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;            
        }
        
        [Fact]
        public async Task TestGet()
        {
            // Arrange
            
            //empty testdb
            var weatherContext = new WeatherForecastContext(dbContextOptions);
            weatherContext.WeatherForecastItems.RemoveRange(weatherContext.WeatherForecastItems);
            await weatherContext.SaveChangesAsync();
            Assert.Equal(0, await weatherContext.WeatherForecastItems.CountAsync());

            //testWeatherForecastItemsFromOpenMeteo is a forecast from today for 14 days
            testWeatherForecastItemsFromOpenMeteo = new List<WeatherForecastItem>();
            start = DateOnly.FromDateTime(DateTime.Now);
            for (var i = 0; i < 14; i++)
            {
                WeatherForecastItem item = new WeatherForecastItem()
                {
                    Date = start.AddDays(i),
                    TemperatureCNight = i * 4,
                    TemperatureCMorning = i * 4 + 1,
                    TemperatureCAfternoon = i * 4 + 2,
                    TemperatureCEvening = i * 4 + 3,
                };
                testWeatherForecastItemsFromOpenMeteo.Add(item);
            }

            // initial test data for `get` query contains first and last days of 14-days period from today
            start = DateOnly.FromDateTime(DateTime.Now);
            end = start.AddDays(13);
            dbInitialTestData = new List<WeatherForecastItem>()
                {
                    new WeatherForecastItem()
                    {
                        Date = start,
                        TemperatureCNight = -1,
                        TemperatureCMorning = 0,
                        TemperatureCAfternoon = 1,
                        TemperatureCEvening = 2
                    },
                    new WeatherForecastItem()
                    {
                        Date = end,
                        TemperatureCNight = 3,
                        TemperatureCMorning = 4,
                        TemperatureCAfternoon = 5,
                        TemperatureCEvening = 6
                    }
                };
            dbTestDataAfterQuery = new List<WeatherForecastItem>();
            dbTestDataAfterQuery.AddRange(testWeatherForecastItemsFromOpenMeteo);
            dbTestDataAfterQuery[0] = dbInitialTestData[0];
            dbTestDataAfterQuery[13] = dbInitialTestData[1];

            //add and save initial test data in testdb
            weatherContext.AddRange(dbInitialTestData);
            weatherContext.SaveChanges();
            Assert.Equal(2, await weatherContext.WeatherForecastItems.CountAsync());
            
            //mock logger and queries to api open-meteo
            var logger = Mock.Of<ILogger<WeatherForecastItemsController>>();
            Mock<IExternalWeatherForecast> externalWeatherForecast = new Mock<IExternalWeatherForecast>();
           
            externalWeatherForecast
                .Setup(s => s.GetWeatherForecastAsync(start, end))
                .ReturnsAsync(testWeatherForecastItemsFromOpenMeteo);
            
            //create controller for testing
            WeatherForecastItemsController repository = new WeatherForecastItemsController(weatherContext, logger, externalWeatherForecast.Object);

            // Act
            var result = await repository.GetWeatherForecast();

            // Assert
            Assert.NotEqual(testWeatherForecastItemsFromOpenMeteo, result.ToList());
            Assert.Equal(dbTestDataAfterQuery, result.ToList());
            Assert.Equal(14, await weatherContext.WeatherForecastItems.CountAsync());
            Assert.Equal(dbTestDataAfterQuery, await weatherContext.WeatherForecastItems.OrderBy(x => x.Date).ToListAsync());
        }
        
        [Fact]
        public async Task TestPost()
        {
            // Arrange
            period = "{ \"fromDate\": \"2023-04-01\",\"toDate\": \"2023-04-03\"}";
            start = new DateOnly(2023, 04, 1);
            end = new DateOnly(2023, 04, 3);

            // initial test data for `post` query, contains 1-st April and 11-th April
            // 1-April should change, 11-th April shouldnt
            // should be added 2-3 of April
           
            dbInitialTestData = new List<WeatherForecastItem>()
                {
                    new WeatherForecastItem()
                    {
                        Date = start,
                        TemperatureCNight = -1,
                        TemperatureCMorning = 0,
                        TemperatureCAfternoon = 1,
                        TemperatureCEvening = 2
                    },
                    new WeatherForecastItem()
                    {
                        Date = start.AddDays(10),
                        TemperatureCNight = 3,
                        TemperatureCMorning = 4,
                        TemperatureCAfternoon = 5,
                        TemperatureCEvening = 6
                    }
                };

            testWeatherForecastItemsFromOpenMeteo = new List<WeatherForecastItem>()
                {
                    new WeatherForecastItem()
                    {
                        Date = start,
                        TemperatureCNight = 1,
                        TemperatureCMorning = 2,
                        TemperatureCAfternoon = 3,
                        TemperatureCEvening = 4
                    },
                    new WeatherForecastItem()
                    {
                        Date = start.AddDays(1),
                        TemperatureCNight = 5,
                        TemperatureCMorning = 6,
                        TemperatureCAfternoon = 7,
                        TemperatureCEvening = 8
                    },
                    new WeatherForecastItem()
                    {
                        Date = start.AddDays(2),
                        TemperatureCNight = 9,
                        TemperatureCMorning = 10,
                        TemperatureCAfternoon = 11,
                        TemperatureCEvening = 12
                    }
                };
            
            dbTestDataAfterQuery = new List<WeatherForecastItem>();
            dbTestDataAfterQuery.AddRange(testWeatherForecastItemsFromOpenMeteo);
            dbTestDataAfterQuery.Add(dbInitialTestData.Last());



            var weatherContext = new WeatherForecastContext(dbContextOptions);
            weatherContext.WeatherForecastItems.RemoveRange(weatherContext.WeatherForecastItems);
            await weatherContext.SaveChangesAsync();
            Assert.Equal(0, await weatherContext.WeatherForecastItems.CountAsync());
            
            weatherContext.AddRange(dbInitialTestData);
            weatherContext.SaveChanges();
            Assert.Equal(dbInitialTestData.Count, await weatherContext.WeatherForecastItems.CountAsync());

            var logger = Mock.Of<ILogger<WeatherForecastItemsController>>();
            Mock<IExternalWeatherForecast> externalWeatherForecast = new Mock<IExternalWeatherForecast>();

            externalWeatherForecast
                .Setup(s => s.GetWeatherForecastAsync(start, end))
                .ReturnsAsync(testWeatherForecastItemsFromOpenMeteo);

            WeatherForecastItemsController repository = new WeatherForecastItemsController(weatherContext, logger, externalWeatherForecast.Object);


            // Act
            await repository.PostWeatherForecastItem(period);
            var res = await weatherContext.WeatherForecastItems.OrderBy(x => x.Date).ToListAsync();
            // Assert
            Assert.Equal(dbTestDataAfterQuery, res);
        }

    }
}