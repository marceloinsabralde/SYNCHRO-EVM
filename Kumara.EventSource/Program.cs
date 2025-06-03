// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.DbContext;
using Kumara.EventSource.Extensions;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.OpenApi;
using Kumara.EventSource.Repositories;
using Kumara.EventSource.Utilities;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

JsonSchemaGenerator.GenerateJsonSchemas();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEventSourceOpenApi();

builder.Services.AddScoped<IEventRepository, EventRepositoryMongo>();
builder.Services.AddScoped<IEventValidator, EventValidator>();
builder.Services.AddSingleton<Dictionary<string, Type>>(
    EventTypeMapInitializer.InitializeEventTypeMap()
);

builder.Services.AddMongoDbContext(builder.Configuration);

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = KumaraJsonOptions
            .DefaultOptions
            .PropertyNamingPolicy;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = KumaraJsonOptions
            .DefaultOptions
            .PropertyNameCaseInsensitive;
        options.JsonSerializerOptions.DefaultIgnoreCondition = KumaraJsonOptions
            .DefaultOptions
            .DefaultIgnoreCondition;
    });

WebApplication app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SYNCHRO EVM EventSource API v1");
        c.RoutePrefix = "swagger";
        // Disable resource loading
        c.InjectStylesheet("");
    });
}

app.UseHttpsRedirection();

app.MapControllers();

await app.RunAsync(CancellationToken.None);

public partial class Program { }
