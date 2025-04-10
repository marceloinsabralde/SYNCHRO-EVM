// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Repositories;
using Kumara.EventSource.Utilities;

// Generate JSON schemas for event models
JsonSchemaGenerator.GenerateJsonSchemas();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IEventRepository, EventRepositoryInMemoryList>();

// Register the event validator
builder.Services.AddScoped<IEventValidator, EventValidator>();

builder.Services.AddSingleton<Dictionary<string, Type>>(
    EventTypeMapInitializer.InitializeEventTypeMap()
);

builder.Services.AddControllers();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
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
