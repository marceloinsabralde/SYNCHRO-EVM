// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.DbContext;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Repositories;
using Kumara.EventSource.Utilities;

JsonSchemaGenerator.GenerateJsonSchemas();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddScoped<IEventRepository, EventRepositoryMongo>();

builder.Services.AddScoped<IEventValidator, EventValidator>();

builder.Services.AddSingleton<Dictionary<string, Type>>(
    EventTypeMapInitializer.InitializeEventTypeMap()
);

builder.Services.AddMongoDbContext(builder.Configuration);

builder.Services.AddControllers();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsEnvironment("Test"))
{
    app.UseHttpsRedirection();
}

app.MapControllers();

await app.RunAsync();

public partial class Program { }
