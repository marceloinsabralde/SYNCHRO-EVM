// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net.Http.Headers;
using System.Text.Json;

using CloudNative.CloudEvents;
using CloudNative.CloudEvents.AspNetCore;
using CloudNative.CloudEvents.Http;
using CloudNative.CloudEvents.SystemTextJson;

using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Repositories;

using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IEventRepository, EventRepositoryInMemoryList>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/events", async (HttpContext context, [FromServices] IEventRepository? eventRepository) =>
{
    var formatter = new JsonEventFormatter();
    List<CloudEvent> cloudEvents = new();

    var mediaType = MediaTypeHeaderValue.Parse(context.Request.ContentType ?? "text/plain").MediaType;
    switch (mediaType)
    {
        case "application/cloudevents+json":
            var cloudEvent = await context.Request.ToCloudEventAsync(formatter);
            cloudEvents.Add(cloudEvent);
            break;
        case "application/cloudevents-batch+json":
            cloudEvents = (await context.Request.ToCloudEventBatchAsync(formatter)).ToList();
            break;
        default:
            return Results.Problem(JsonSerializer.Serialize(CreateUnsupportedMediaTypeProblemDetails(context.Request.Path)), statusCode: StatusCodes.Status415UnsupportedMediaType);
    }

    if (cloudEvents.Count != 0 && eventRepository is not null)
    {
        // Add events to the repository
        await eventRepository.AddEventsAsync(cloudEvents);
    }
    return Results.Ok(new { count = cloudEvents.Count });
});

app.MapGet("/events", async (HttpContext context, [FromServices] IEventRepository? eventRepository) =>
{
    var cloudEvents = eventRepository is not null
        ? await eventRepository.GetAllEventsAsync()
        : Enumerable.Empty<CloudEvent>().AsQueryable();
    var formatter = new JsonEventFormatter();
    var content = formatter.EncodeBatchModeMessage(cloudEvents.ToList(), out var contentType);
    context.Response.ContentType = contentType.ToString();
    await context.Response.Body.WriteAsync(content);
});

app.Run();


static Microsoft.AspNetCore.Mvc.ProblemDetails CreateUnsupportedMediaTypeProblemDetails(string path)
{
    return new ProblemDetails
    {
        Status = StatusCodes.Status415UnsupportedMediaType,
        Title = "Unsupported Media Type",
        Detail = "The provided content type is not supported. Please use 'application/cloudevents+json' or 'application/cloudevents-batch+json'.",
        Instance = path,
        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.13"
    };
}

public partial class Program { }
