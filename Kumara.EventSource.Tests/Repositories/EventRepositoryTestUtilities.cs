// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.Models;
using Kumara.EventSource.Tests.Factories;

namespace Kumara.EventSource.Tests.Repositories;

/// <summary>
/// Provides utility methods for repository test data generation.
/// </summary>
public static class EventRepositoryTestUtilities
{
    private static readonly ControlAccountCreatedV1Factory _controlAccountFactory = new();
    private static readonly RandomEventFactory _randomEventFactory = new();

    /// <summary>
    /// Gets a list of test events with different types and data structures.
    /// </summary>
    public static List<Event> GetTestEvents()
    {
        return new List<Event>
        {
            _controlAccountFactory.CreateValid(),
            _randomEventFactory.WithCustomProperties(evt => evt.Type = "some.random.event"),
            _randomEventFactory.WithCustomProperties(evt => evt.Type = "other.random.event"),
        };
    }

    /// <summary>
    /// Builds pagination links for a paginated list response.
    /// </summary>
    /// <param name="paginatedList">The paginated list to build links for.</param>
    /// <param name="eventType">The event type being queried.</param>
    /// <param name="continuationToken">Optional continuation token for next page.</param>
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
