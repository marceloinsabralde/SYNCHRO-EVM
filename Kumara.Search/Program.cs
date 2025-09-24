// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Elastic.Clients.Elasticsearch;
using Kumara.Common.Database;
using Kumara.Common.Extensions;
using Kumara.Search.Database;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables(
    prefix: $"{builder.Environment.EnvironmentName.ToUpper()}_"
);

builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("KumaraSearchDB"),
        npgsqlOptions =>
        {
            npgsqlOptions.SetPostgresVersion(16, 4);
            npgsqlOptions.UseNodaTime();
        }
    );
    options.UseKumaraCommon();
    if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Test"))
    {
        options.EnableSensitiveDataLogging();
    }
});

var elasticsearchUrl =
    builder.Configuration.GetConnectionString("KumaraSearchES")
    ?? throw new InvalidOperationException("Connection string 'KumaraSearchES' is not configured.");

builder.Services.AddSingleton(serviceProvider =>
{
    var kumaraSearchUrl = new Uri(elasticsearchUrl);
    var settings = new ElasticsearchClientSettings(kumaraSearchUrl);
    return new ElasticsearchClient(settings);
});

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

builder
    .Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddElasticsearch(elasticsearchUrl);

builder.Services.AddHttpLogging(options =>
{
    options.CombineLogs = true;
    options.LoggingFields = HttpLoggingFields.All;
    options.RequestBodyLogLimit = 4096;
    options.ResponseBodyLogLimit = 4096;
});

var app = builder.Build();

app.UseHttpsRedirection();

await app.MigrateDbAsync<ApplicationDbContext>();

app.MapControllers();
app.MapHealthChecks("/healthz");

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Test"))
{
    app.UseWhen(
        context => !context.Request.Path.StartsWithSegments("/healthz"),
        builder => builder.UseHttpLogging()
    );
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
