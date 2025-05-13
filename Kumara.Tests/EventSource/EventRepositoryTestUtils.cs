// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSource.Models;
using Kumara.EventSource.Models.Events;

namespace Kumara.Tests.EventSource;

public static class EventRepositoryTestUtils
{
    public static List<Event> GetTestEvents()
    {
        return new List<Event>
        {
            new()
            {
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "kumara.test.event",
                DataJson = JsonSerializer.SerializeToDocument(
                    new TestCreatedV1
                    {
                        TestString = "Test String",
                        TestEnum = TestOptions.OptionA,
                        TestInteger = 100,
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
