// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Database;
using Kumara.Common.Extensions;
using Kumara.Common.Providers;
using Kumara.WebApi.Database;
using Kumara.WebApi.Repositories;
using Kumara.WebApi.Types;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables(
    prefix: $"{builder.Environment.EnvironmentName.ToUpper()}_"
);

builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("KumaraWebApiDB"),
        npgsqlOptions =>
        {
            npgsqlOptions.SetPostgresVersion(16, 4);
            npgsqlOptions.UseNodaTime();
        }
    );
    options.UseKumaraCommon();
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
});

// temporary until we talk to the real iTwin APIs
builder.Services.AddTransient<
    Bentley.ConnectCoreLibs.Providers.Abstractions.Interfaces.IITwinProvider,
    Kumara.WebApi.Providers.FakeITwinProvider
>();

builder.Services.AddScoped<ISettingsDbContext<Settings, SettingKey>>(provider =>
    provider.GetRequiredService<ApplicationDbContext>()
);
builder.Services.AddTransient<IITwinPathProvider, ITwinPathProvider>();
builder.Services.AddTransient<SettingsRepository<Settings, SettingKey>>();

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        options.UseKumaraCommon();
    });

builder.Services.AddOpenApi(options =>
{
    options.UseKumaraCommon();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.UseKumaraCommon();
});

builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

builder.Services.AddHttpLogging(options =>
{
    options.CombineLogs = true;
    options.LoggingFields = HttpLoggingFields.All;
    options.RequestBodyLogLimit = 4096;
    options.ResponseBodyLogLimit = 4096;
});

var openTelBuilder = builder
    .Services.AddOpenTelemetry()
    .UseOtlpExporter()
    .WithTracing(tracing =>
        tracing.AddAspNetCoreInstrumentation().AddEntityFrameworkCoreInstrumentation().AddNpgsql()
    )
    .WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation().AddNpgsqlInstrumentation());

if (!builder.Environment.IsDevelopment())
{
    openTelBuilder.WithLogging(logging => logging.AddConsoleExporter());
}

WebApplication app = builder.Build();

app.UseHttpsRedirection();

await app.MigrateDbAsync<ApplicationDbContext>();

app.MapControllers();
app.MapHealthChecks("/healthz");

if (app.Environment.IsDevelopment())
{
    app.SeedDevelopmentData();
    app.UseHttpLogging();
}
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Test"))
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.EnableDeepLinking();
    });
}

app.Run();
