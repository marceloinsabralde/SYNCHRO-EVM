// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.AspNetCore;
using CloudNative.CloudEvents.Http;
using CloudNative.CloudEvents.SystemTextJson;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/events", async (HttpContext context) =>
{
    var cloudEvents = await context.Request.ToCloudEventBatchAsync(new JsonEventFormatter());
    return Results.Ok(new { cloudEvents.Count });
});

app.MapGet("/events", (HttpContext context) =>
{
    var cloudEvents = new List<CloudEvent>
    {
        new()
        {
            Id = Guid.NewGuid().ToString(),
            Source = new Uri("https://example.com/source"),
            Type = "com.example.type",
            Time = DateTimeOffset.UtcNow,
            DataContentType = "application/json",
            Data = new { message = "first dummy event" }
        },
        new()
        {
            Id = Guid.NewGuid().ToString(),
            Source = new Uri("https://example.com/source"),
            Type = "com.example.type",
            Time = DateTimeOffset.UtcNow,
            DataContentType = "application/json",
            Data = new { message = "second dummy event" }
        }
    };

    var formatter = new JsonEventFormatter();
    var content = formatter.EncodeBatchModeMessage(cloudEvents, out var contentType);

    context.Response.ContentType = contentType.ToString();
    return context.Response.Body.WriteAsync(content);
});

app.Run();

public partial class Program { }
