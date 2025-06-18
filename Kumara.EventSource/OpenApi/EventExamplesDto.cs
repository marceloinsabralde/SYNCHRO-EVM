// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using System.Text.Json.Serialization;
using Kumara.EventSource.Models;
using Kumara.EventSource.Models.Events;
using Swashbuckle.AspNetCore.Filters;

namespace Kumara.EventSource.OpenApi;

/// <summary>
/// DTO class for Event that uses a JSON object for data instead of JsonDocument for Swagger examples
/// </summary>
public class EventDto
{
    [JsonPropertyName("iTwinId")]
    public Guid ITwinId { get; set; }

    [JsonPropertyName("accountId")]
    public Guid AccountId { get; set; }

    [JsonPropertyName("correlationId")]
    public string CorrelationId { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("specVersion")]
    public string SpecVersion { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public Uri Source { get; set; } = new("https://example.com");

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public object Data { get; set; } = new();

    /// <summary>
    /// Create a DTO from an Event with properly structured data
    /// </summary>
    public static EventDto FromEvent(Event @event)
    {
        return new EventDto
        {
            ITwinId = @event.ITwinId,
            AccountId = @event.AccountId,
            CorrelationId = @event.CorrelationId,
            Id = @event.Id,
            SpecVersion = @event.SpecVersion,
            Source = @event.Source,
            Type = @event.Type,
            Data =
                @event.Type switch
                {
                    "control.account.created.v1" =>
                        JsonSerializer.Deserialize<ControlAccountCreatedV1>(@event.DataJson),
                    "control.account.updated.v1" =>
                        JsonSerializer.Deserialize<ControlAccountUpdatedV1>(@event.DataJson),
                    _ => JsonSerializer.Deserialize<object>(@event.DataJson),
                } ?? new object(),
        };
    }
}

/// <summary>
/// DTO class for PaginatedList that uses EventDto instead of Event
/// </summary>
public class PaginatedListDto<T>
{
    public List<T> Items { get; set; } = new();
    public PaginationLinks Links { get; set; } = new();
}

/// <summary>
/// Provides examples of EventDto objects for the POST /events endpoint in Swagger UI
/// </summary>
public class CreateEventsRequestExampleDto : IExamplesProvider<List<EventDto>>
{
    public List<EventDto> GetExamples()
    {
        DateTimeOffset now = DateTimeOffset.Now;
        Guid iTwinId = Guid.NewGuid();
        Guid accountId = Guid.NewGuid();

        Guid controlAccountId = Guid.NewGuid();

        EventDto createdEvent = new()
        {
            ITwinId = iTwinId,
            AccountId = accountId,
            CorrelationId = Guid.NewGuid().ToString(),
            Id = Guid.NewGuid(),
            SpecVersion = "1.0",
            Source = new Uri("https://example.com/construction/events"),
            Type = "control.account.created.v1",
            Data = new ControlAccountCreatedV1
            {
                Id = controlAccountId,
                Name = "Main Building Foundation",
                WbsPath = "1.2.3",
                TaskId = Guid.NewGuid(),
                PlannedStart = now,
                PlannedFinish = now.AddDays(30),
                ActualStart = now,
                ActualFinish = null,
            },
        };

        EventDto updatedEvent = new()
        {
            ITwinId = iTwinId,
            AccountId = accountId,
            CorrelationId = Guid.NewGuid().ToString(),
            Id = Guid.NewGuid(),
            SpecVersion = "1.0",
            Source = new Uri("https://example.com/construction/events"),
            Type = "control.account.updated.v1",
            Data = new ControlAccountUpdatedV1
            {
                Id = controlAccountId,
                Name = "Main Building Foundation - Phase 1",
                WbsPath = "1.2.3",
                TaskId = Guid.NewGuid(),
                PlannedStart = now,
                PlannedFinish = now.AddDays(45),
                ActualStart = now,
                ActualFinish = null,
            },
        };

        return new List<EventDto> { createdEvent, updatedEvent };
    }
}

public class GetEventsResponseExampleDto : IExamplesProvider<PaginatedListDto<EventDto>>
{
    public PaginatedListDto<EventDto> GetExamples()
    {
        DateTimeOffset now = DateTimeOffset.Now;
        Guid iTwinId = Guid.NewGuid();
        Guid accountId = Guid.NewGuid();
        Guid controlAccountId = Guid.NewGuid();

        EventDto createdEvent = new()
        {
            ITwinId = iTwinId,
            AccountId = accountId,
            CorrelationId = Guid.NewGuid().ToString(),
            Id = Guid.NewGuid(),
            SpecVersion = "1.0",
            Source = new Uri("https://example.com/construction/events"),
            Type = "control.account.created.v1",
            Data = new ControlAccountCreatedV1
            {
                Id = controlAccountId,
                Name = "Main Building Foundation",
                WbsPath = "1.2.3",
                TaskId = Guid.NewGuid(),
                PlannedStart = now,
                PlannedFinish = now.AddDays(30),
                ActualStart = now,
                ActualFinish = null,
            },
        };

        EventDto updatedEvent = new()
        {
            ITwinId = iTwinId,
            AccountId = accountId,
            CorrelationId = Guid.NewGuid().ToString(),
            Id = Guid.NewGuid(),
            SpecVersion = "1.0",
            Source = new Uri("https://example.com/construction/events"),
            Type = "control.account.updated.v1",
            Data = new ControlAccountUpdatedV1
            {
                Id = controlAccountId,
                Name = "Main Building Foundation - Phase 1",
                WbsPath = "1.2.3",
                TaskId = Guid.NewGuid(),
                PlannedStart = now,
                PlannedFinish = now.AddDays(45),
                ActualStart = now,
                ActualFinish = null,
            },
        };

        PaginatedListDto<EventDto> result = new()
        {
            Items = new List<EventDto> { createdEvent, updatedEvent },
        };

        // Set links for pagination example
        string baseUrl = "https://api.example.com/events";
        string continuationToken = "dG9rZW4="; // Base64 encoded example token

        // Set up links manually since we're just providing an example
        result.Links.Self = new PaginationLink { Href = $"{baseUrl}?iTwinId={iTwinId}" };
        result.Links.Next = new PaginationLink
        {
            Href = $"{baseUrl}?continuationtoken={continuationToken}&iTwinId={iTwinId}",
        };

        return result;
    }
}
