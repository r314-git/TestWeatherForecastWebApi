using Microsoft.EntityFrameworkCore;
using TestWebApplication.AdditionalClasses;
using TestWebApplication.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

//set connection string to database from configuration.
//can be found in `appsettings.json` file (`appsettings.Development.json`)
//add types `DateOnly` and `TimeOnly` for EF and SQL (nuget package EFCore.SqlServer.DateOnlyTimeOnly)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<WeatherForecastContext>(options => options.UseSqlServer(connectionString, x => x.UseDateOnlyTimeOnly()));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IExternalWeatherForecast, ExternalWeatherForecastOpenMeteo>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{ 
    app.UseExceptionHandler("/Error"); 
}

app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
