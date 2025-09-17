// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Database;
using Kumara.Common.Extensions;
using Kumara.EventSource.Database;
using Kumara.EventSource.Services;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureBentleyProtectedApi();

builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("KumaraEventSourceDB"),
        npgsqlOptions =>
        {
            npgsqlOptions.SetPostgresVersion(16, 9);
            npgsqlOptions.UseNodaTime();
        }
    );

    options.UseKumaraCommon();

    if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Test"))
    {
        options.EnableSensitiveDataLogging();
    }

    if (builder.Environment.IsDevelopment())
    {
        options.UseSeeding(DbSeeder.SeedData);
        options.UseAsyncSeeding(DbSeeder.SeedDataAsync);
    }
});

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.UseKumaraCommon();
    });

builder.Services.AddHostedService<IdempotencyKeyCleanupService>();
builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();
builder.Services.AddOpenApi(options => options.UseKumaraCommon());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.UseKumaraCommon());

builder.Services.AddHttpLogging(options =>
{
    options.CombineLogs = true;
    options.LoggingFields = HttpLoggingFields.All;
    options.RequestBodyLogLimit = 4096;
    options.ResponseBodyLogLimit = 4096;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseHttpLogging();
}
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options => options.EnableDeepLinking());
}

app.UseHttpsRedirection();

await app.MigrateDbAsync<ApplicationDbContext>();

app.UseAuthentication().UseAuthorization();
app.MapControllers().RequireAuthorization();
app.MapHealthChecks("/healthz").AllowAnonymous();

app.Run();
