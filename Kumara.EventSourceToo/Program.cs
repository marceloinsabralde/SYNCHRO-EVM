// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Database;
using Kumara.Common.Extensions;
using Kumara.EventSourceToo.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging();
});

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.UseKumaraCommon();
    });

builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();
builder.Services.AddOpenApi(options => options.UseKumaraCommon());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.UseKumaraCommon());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options => options.EnableDeepLinking());
}

app.UseHttpsRedirection();

await app.MigrateDbAsync<ApplicationDbContext>();

app.MapControllers();
app.MapHealthChecks("/healthz");

app.Run();
