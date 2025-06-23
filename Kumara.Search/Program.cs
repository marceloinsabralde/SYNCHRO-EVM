// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Database;
using Kumara.Search.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables(
    prefix: $"{builder.Environment.EnvironmentName.ToUpper()}_"
);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.UseAllOfToExtendReferenceSchemas();
    options.EnableAnnotations();
});

builder.Services.AddDbContextPool<ApplicationDbContext>(opt =>
    opt.UseNpgsql(
            builder.Configuration.GetConnectionString("KumaraSearchDB"),
            o => o.SetPostgresVersion(16, 4).UseNodaTime()
        )
        .UseSnakeCaseNamingConvention()
);

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Test"))
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

await app.MigrateDbAsync<ApplicationDbContext>();

app.Run();

public partial class Program { }
