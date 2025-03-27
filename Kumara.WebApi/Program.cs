// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using Kumara.Database;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContextPool<ApplicationDbContext>(opt =>
    opt.UseNpgsql(
            builder.Configuration.GetConnectionString("PerformNextGen"),
            o => o.SetPostgresVersion(16, 4).UseNodaTime()
        )
        .UseSnakeCaseNamingConvention()
);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Learn more about configuring HTTP Logging at https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-logging/?view=aspnetcore-9.0
builder.Services.AddHttpLogging(options =>
{
    options.CombineLogs = true;
    options.LoggingFields = HttpLoggingFields.All;
    options.RequestBodyLogLimit = 4096;
    options.ResponseBodyLogLimit = 4096;
});

// Learn about configuring OpenTelemetry at https://opentelemetry.io/docs/languages/net/
builder
    .Services.AddOpenTelemetry()
    .UseOtlpExporter() // Use the OpenTelemetry Protocol (OTLP) exporter for all signals
    .WithTracing(tracing =>
        tracing.AddAspNetCoreInstrumentation().AddEntityFrameworkCoreInstrumentation().AddNpgsql()
    )
    .WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation().AddNpgsqlInstrumentation())
    .WithLogging(logging => logging.AddConsoleExporter());

WebApplication app = builder.Build();

if (!app.Environment.IsEnvironment("Test"))
{
    app.UseHttpsRedirection();
}

app.RunMigrations();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpLogging();
}

app.Run();
