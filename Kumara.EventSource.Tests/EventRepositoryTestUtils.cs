// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSource.Models;
using Kumara.EventSource.Models.Events;

namespace Kumara.Tests.EventSource;

public static class EventRepositoryTestUtils
{
    public static DateTimeOffset GetTestDateTimeOffset()
    {
        return new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
    }

    public static List<Event> GetTestEvents()
    {
        DateTimeOffset now = EventRepositoryTestUtils.GetTestDateTimeOffset();

        return new List<Event>
        {
            new()
            {
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "control.account.created.v1",
                DataJson = JsonSerializer.SerializeToDocument(
                    new ControlAccountCreatedV1
                    {
                        Id = Guid.NewGuid(),
                        Name = "Test Account",
                        WbsPath = "1.2.3",
                        TaskId = Guid.NewGuid(),
                        PlannedStart = now,
                        PlannedFinish = now.AddDays(10),
                        ActualStart = now,
                        ActualFinish = now.AddDays(9),
                    }
                ),
            },
            new()
            {
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "some.random.event",
                DataJson = JsonSerializer.SerializeToDocument(
                    new
                    {
                        RandomString = "RandomValue",
                        RandomNumber = 42,
                        NestedObject = new
                        {
                            NestedKey = "NestedValue",
                            NestedArray = new[] { 1, 2, 3 },
                        },
                    }
                ),
            },
            new()
            {
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "other.random.event",
                DataJson = JsonSerializer.SerializeToDocument(
                    new
                    {
                        RandomKey = "RandomValue",
                        NestedObject = new
                        {
                            NestedKey = "NestedValue",
                            NestedArray = new[] { 1, 2, 3 },
                        },
                        RandomNumber = 42,
                    }
                ),
            },
        };
    }

    public static void BuildPaginationLinks<T>(
        PaginatedList<T> paginatedList,
        string eventType,
        string? continuationToken = null
    )
    {
        string baseUrl = "http://test-api.com/events";
        string selfQuery = $"?type={eventType}";
        paginatedList.Links.Self = new PaginationLink { Href = baseUrl + selfQuery };

        if (continuationToken != null)
        {
            string nextQuery = $"{selfQuery}&continuationToken={continuationToken}";
            paginatedList.Links.Next = new PaginationLink { Href = baseUrl + nextQuery };
        }
    }
}
