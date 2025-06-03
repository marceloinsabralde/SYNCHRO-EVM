// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSource.Models;
using Kumara.EventSource.Models.Events;
using Swashbuckle.AspNetCore.Filters;

namespace Kumara.EventSource.OpenApi;

/// <summary>
/// Provides examples of Event objects for the POST /events endpoint in Swagger UI
/// </summary>
public class PostEventsRequestExample : IExamplesProvider<List<Event>>
{
    public List<Event> GetExamples()
    {
        DateTimeOffset now = DateTimeOffset.Now;
        Guid iTwinId = Guid.NewGuid();
        Guid accountId = Guid.NewGuid();

        Guid controlAccountId = Guid.NewGuid();

        return new List<Event>
        {
            new()
            {
                ITwinId = iTwinId,
                AccountId = accountId,
                CorrelationId = Guid.NewGuid().ToString(),
                Id = Guid.NewGuid(),
                SpecVersion = "1.0",
                Source = new Uri("https://example.com/construction/events"),
                Type = "control.account.created.v1",
                DataJson = JsonSerializer.SerializeToDocument(
                    new ControlAccountCreatedV1
                    {
                        Id = controlAccountId,
                        Name = "Main Building Foundation",
                        WbsPath = "1.2.3",
                        TaskId = Guid.NewGuid(),
                        PlannedStart = now,
                        PlannedFinish = now.AddDays(30),
                        ActualStart = now,
                        ActualFinish = null,
                    }
                ),
            },
            new()
            {
                ITwinId = iTwinId,
                AccountId = accountId,
                CorrelationId = Guid.NewGuid().ToString(),
                Id = Guid.NewGuid(),
                SpecVersion = "1.0",
                Source = new Uri("https://example.com/construction/events"),
                Type = "control.account.updated.v1",
                DataJson = JsonSerializer.SerializeToDocument(
                    new ControlAccountUpdatedV1
                    {
                        Id = controlAccountId,
                        Name = "Main Building Foundation - Phase 1",
                        WbsPath = "1.2.3",
                        TaskId = Guid.NewGuid(),
                        PlannedStart = now,
                        PlannedFinish = now.AddDays(45),
                        ActualStart = now,
                        ActualFinish = null,
                    }
                ),
            },
        };
    }
}

/// <summary>
/// Provides examples of paginated Event responses for the GET /events endpoint in Swagger UI
/// </summary>
public class GetEventsResponseExample : IExamplesProvider<PaginatedList<Event>>
{
    public PaginatedList<Event> GetExamples()
    {
        DateTimeOffset now = DateTimeOffset.Now;
        Guid iTwinId = Guid.NewGuid();
        Guid accountId = Guid.NewGuid();

        Guid controlAccountId = Guid.NewGuid();

        List<Event> events = new()
        {
            new Event
            {
                ITwinId = iTwinId,
                AccountId = accountId,
                CorrelationId = Guid.NewGuid().ToString(),
                Id = Guid.NewGuid(),
                SpecVersion = "1.0",
                Source = new Uri("https://example.com/construction/events"),
                Type = "control.account.created.v1",
                DataJson = JsonSerializer.SerializeToDocument(
                    new ControlAccountCreatedV1
                    {
                        Id = controlAccountId,
                        Name = "Main Building Foundation",
                        WbsPath = "1.2.3",
                        TaskId = Guid.NewGuid(),
                        PlannedStart = now,
                        PlannedFinish = now.AddDays(30),
                        ActualStart = now,
                        ActualFinish = null,
                    }
                ),
            },
            new Event
            {
                ITwinId = iTwinId,
                AccountId = accountId,
                CorrelationId = Guid.NewGuid().ToString(),
                Id = Guid.NewGuid(),
                SpecVersion = "1.0",
                Source = new Uri("https://example.com/construction/events"),
                Type = "control.account.updated.v1",
                DataJson = JsonSerializer.SerializeToDocument(
                    new ControlAccountUpdatedV1
                    {
                        Id = controlAccountId,
                        Name = "Main Building Foundation - Phase 1",
                        WbsPath = "1.2.3",
                        TaskId = Guid.NewGuid(),
                        PlannedStart = now,
                        PlannedFinish = now.AddDays(45),
                        ActualStart = now,
                        ActualFinish = null,
                    }
                ),
            },
        };

        PaginatedList<Event> result = new() { Items = events, HasMoreItems = true };

        // Set links for pagination example
        string baseUrl = "https://api.example.com/events";
        Dictionary<string, string> queryParams = new() { { "iTwinId", iTwinId.ToString() } };
        string continuationToken = "dG9rZW4="; // Base64 encoded example token

        // Set up links manually since we're just providing an example
        result.HasMoreItems = true;
        result.Links.Self = new PaginationLink { Href = $"{baseUrl}?iTwinId={iTwinId}" };
        result.Links.Next = new PaginationLink
        {
            Href = $"{baseUrl}?continuationtoken={continuationToken}&iTwinId={iTwinId}",
        };

        return result;
    }
}
