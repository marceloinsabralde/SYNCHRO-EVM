// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Database;
using Kumara.Core.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables(
    prefix: $"{builder.Environment.EnvironmentName.ToUpper()}_"
);

builder.Services.AddOpenApi();

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
    options.UseSnakeCaseNamingConvention();
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.UseAllOfToExtendReferenceSchemas();
    options.EnableAnnotations();
});

var app = builder.Build();

await app.MigrateDbAsync<ApplicationDbContext>();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Test"))
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
