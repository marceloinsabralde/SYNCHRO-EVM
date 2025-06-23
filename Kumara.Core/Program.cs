// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Database;
using Kumara.Common.Extensions;
using Kumara.Core.Database;
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
        builder.Configuration.GetConnectionString("KumaraCoreDB"),
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

var app = builder.Build();

app.UseHttpsRedirection();

await app.MigrateDbAsync<ApplicationDbContext>();

app.MapControllers();
app.MapHealthChecks("/healthz");

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
