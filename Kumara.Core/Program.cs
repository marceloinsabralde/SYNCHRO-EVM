// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Database;
using Kumara.Core.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables(
    prefix: $"{builder.Environment.EnvironmentName.ToUpper()}_"
);

builder.Services.AddOpenApi();

builder.Services.AddDbContextPool<ApplicationDbContext>(opt =>
    opt.UseNpgsql(
            builder.Configuration.GetConnectionString("KumaraCoreDB"),
            o => o.SetPostgresVersion(16, 4).UseNodaTime()
        )
        .UseSnakeCaseNamingConvention()
);

var app = builder.Build();

await app.MigrateDbAsync<ApplicationDbContext>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing",
    "Bracing",
    "Chilly",
    "Cool",
    "Mild",
    "Warm",
    "Balmy",
    "Hot",
    "Sweltering",
    "Scorching",
};

app.MapGet(
        "/weatherforecast",
        () =>
        {
            var forecast = Enumerable
                .Range(1, 5)
                .Select(index => new WeatherForecast(
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();
            return forecast;
        }
    )
    .WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
