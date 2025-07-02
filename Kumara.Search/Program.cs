// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Elastic.Clients.Elasticsearch;
using Kumara.Common.Database;
using Kumara.Common.Extensions;
using Kumara.Search.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables(
    prefix: $"{builder.Environment.EnvironmentName.ToUpper()}_"
);

builder.Services.AddOpenApi(options =>
{
    options.UseKumaraCommon();
});
builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.UseKumaraCommon();
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.UseKumaraCommon();
});

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
});

builder.Services.AddSingleton(serviceProvider =>
{
    var elasticsearchUrl =
        builder.Configuration.GetConnectionString("KumaraSearchES")
        ?? throw new InvalidOperationException(
            "Connection string 'KumaraSearchES' is not configured."
        );
    var kumaraSearchUrl = new Uri(elasticsearchUrl);
    var settings = new ElasticsearchClientSettings(kumaraSearchUrl);
    return new ElasticsearchClient(settings);
});

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Test"))
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

await app.MigrateDbAsync<ApplicationDbContext>();

app.MapControllers();

app.Run();

public partial class Program { }
