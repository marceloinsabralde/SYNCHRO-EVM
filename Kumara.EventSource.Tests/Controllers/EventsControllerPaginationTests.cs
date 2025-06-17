// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Collections.Specialized;
using System.Text.Json;
using Kumara.EventSource.Models;
using Kumara.EventSource.Tests.Controllers.Helpers;

namespace Kumara.EventSource.Tests.Controllers;

public class EventsControllerPaginationTests : EventsControllerTestBase
{
    [Fact]
    public async Task GetEvents_WithNoPaginationParameters_ReturnsDefaultPaginatedEvents()
    {
        // Create enough events to trigger pagination (more than default page size)
        List<Event> events = new();
        for (int i = 0; i < 60; i++)
        {
            // Add delay to ensure different UUID v7 IDs (time-ordered)
            await Task.Delay(5, TestContext.Current.CancellationToken);

            events.Add(
                new Event
                {
                    ITwinId = Guid.NewGuid(),
                    AccountId = Guid.NewGuid(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    SpecVersion = "1.0",
                    Source = new Uri($"http://example.com/TestSource{i}"),
                    Type = "test.pagination.default",
                    Id = Guid.CreateVersion7(), // UUID v7 for time ordering
                    DataJson = JsonSerializer.SerializeToDocument(
                        new { Index = i, Message = $"Default Pagination Test {i}" }
                    ),
                }
            );
        }

        await _eventRepository.AddEventsAsync(events, TestContext.Current.CancellationToken);

        HttpResponseMessage response = await _client.GetAsync(
            GetEventsEndpoint(),
            TestContext.Current.CancellationToken
        );

        response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken
        );

        // Deserialize as PaginatedEvents
        PaginatedResponseWrapper? paginatedResponse =
            JsonSerializer.Deserialize<PaginatedResponseWrapper>(responseContent, JsonOptions);

        // Verify pagination structure
        paginatedResponse.ShouldNotBeNull();
        paginatedResponse.Items.ShouldNotBeNull();
        paginatedResponse.Links.ShouldNotBeNull();
        paginatedResponse.Links.Self.ShouldNotBeNull();
        paginatedResponse.Links.Next.ShouldNotBeNull(
            "Should have a Next link when more pages exist"
        );
        paginatedResponse.Items.Count.ShouldBe(50); // Default page size
    }

    [Fact]
    public async Task GetEvents_WithPageSize_ReturnsPaginatedEventsWithSpecifiedSize()
    {
        List<Event> events = new();
        for (int i = 0; i < 30; i++)
        {
            await Task.Delay(5, TestContext.Current.CancellationToken);
            events.Add(
                new Event
                {
                    ITwinId = Guid.NewGuid(),
                    AccountId = Guid.NewGuid(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    SpecVersion = "1.0",
                    Source = new Uri($"http://example.com/TestSource{i}"),
                    Type = "test.pagination.pagesize",
                    Id = Guid.CreateVersion7(), // UUID v7 for time ordering
                    DataJson = JsonSerializer.SerializeToDocument(
                        new { Index = i, Message = $"PageSize Test {i}" }
                    ),
                }
            );
        }

        await _eventRepository.AddEventsAsync(events, TestContext.Current.CancellationToken);

        // Set custom page size
        int customPageSize = 15;
        HttpResponseMessage response = await _client.GetAsync(
            GetEventsEndpoint($"top={customPageSize}"),
            TestContext.Current.CancellationToken
        );

        response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken
        );

        PaginatedResponseWrapper? paginatedResponse =
            JsonSerializer.Deserialize<PaginatedResponseWrapper>(responseContent, JsonOptions);

        paginatedResponse.ShouldNotBeNull();
        paginatedResponse.Items.ShouldNotBeNull();
        paginatedResponse.Links.ShouldNotBeNull();
        paginatedResponse.Links.Self.ShouldNotBeNull();
        paginatedResponse.Links.Next.ShouldNotBeNull(
            "Should have a Next link when more pages exist"
        );
    }

    [Fact]
    public async Task GetEvents_WithContinuationToken_ReturnsNextPage()
    {
        List<Event> events = new();
        for (int i = 0; i < 30; i++)
        {
            await Task.Delay(5, TestContext.Current.CancellationToken);
            events.Add(
                new Event
                {
                    ITwinId = Guid.NewGuid(),
                    AccountId = Guid.NewGuid(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    SpecVersion = "1.0",
                    Source = new Uri($"http://example.com/TestSource{i}"),
                    Type = "test.pagination.continuation",
                    Id = Guid.CreateVersion7(), // UUID v7 for time ordering
                    DataJson = JsonSerializer.SerializeToDocument(
                        new { Index = i, Message = $"Continuation Test {i}" }
                    ),
                }
            );
        }

        await _eventRepository.AddEventsAsync(events, TestContext.Current.CancellationToken);

        // Get first page
        int pageSize = 10;
        HttpResponseMessage firstResponse = await _client.GetAsync(
            GetEventsEndpoint($"type=test.pagination.continuation&top={pageSize}"),
            TestContext.Current.CancellationToken
        );
        firstResponse.EnsureSuccessStatusCode();

        string firstResponseContent = await firstResponse.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken
        );

        PaginatedResponseWrapper? firstPage = JsonSerializer.Deserialize<PaginatedResponseWrapper>(
            firstResponseContent,
            JsonOptions
        );

        firstPage.ShouldNotBeNull();
        firstPage.Links.Next.ShouldNotBeNull("First page should have a next link");

        // Get the Next URL which should contain the continuation token
        string nextUrl = firstPage.Links.Next.Href;

        // Extract continuation token from the next link URL
        NameValueCollection queryParams = System.Web.HttpUtility.ParseQueryString(
            new Uri(nextUrl).Query
        );
        string continuationToken = queryParams["continuationtoken"] ?? "";

        continuationToken.ShouldNotBeNullOrEmpty(
            "Should be able to extract continuation token from Next link"
        );

        HttpResponseMessage secondResponse = await _client.GetAsync(
            GetEventsEndpoint($"top={pageSize}&continuationtoken={continuationToken}"),
            TestContext.Current.CancellationToken
        );

        secondResponse.EnsureSuccessStatusCode();
        string secondResponseContent = await secondResponse.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken
        );
        PaginatedResponseWrapper? secondPage = JsonSerializer.Deserialize<PaginatedResponseWrapper>(
            secondResponseContent,
            JsonOptions
        );

        secondPage.ShouldNotBeNull();
        List<Event> secondPageEvents = secondPage.GetEvents();
        secondPageEvents.ShouldNotBeNull();
        secondPageEvents.Count.ShouldBe(pageSize);

        // Ensure no duplicate events between pages
        List<Guid> firstPageIds = firstPage.GetEvents().Select(e => e.Id).ToList();
        List<Guid> secondPageIds = secondPageEvents.Select(e => e.Id).ToList();
        firstPageIds.Intersect(secondPageIds).ShouldBeEmpty();

        // Verify that type filter is preserved from the token
        secondPage
            .Items.All(e => e.Type == "test.pagination.continuation")
            .ShouldBeTrue("The type filter should be preserved from the continuation token");
    }

    [Fact]
    public async Task GetEvents_WithContinuationTokenAndFilters_ReturnsCombinedResult()
    {
        Guid targetITwinId = Guid.NewGuid();
        string eventType = "test.pagination.combined";

        List<Event> events = new();
        for (int i = 0; i < 30; i++)
        {
            await Task.Delay(5, TestContext.Current.CancellationToken);
            events.Add(
                new Event
                {
                    ITwinId = targetITwinId, // Use the same ITwinId for all events
                    AccountId = Guid.NewGuid(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    SpecVersion = "1.0",
                    Source = new Uri($"http://example.com/TestSource{i}"),
                    Type = eventType,
                    Id = Guid.CreateVersion7(), // UUID v7 for time ordering
                    DataJson = JsonSerializer.SerializeToDocument(
                        new { Index = i, Message = $"Combined Filter Test {i}" }
                    ),
                }
            );
        }

        await _eventRepository.AddEventsAsync(events, TestContext.Current.CancellationToken);

        // Get first page with filters
        int pageSize = 10;
        HttpResponseMessage firstResponse = await _client.GetAsync(
            GetEventsEndpoint($"iTwinId={targetITwinId}&type={eventType}&top={pageSize}"),
            TestContext.Current.CancellationToken
        );

        firstResponse.EnsureSuccessStatusCode();
        string firstResponseContent = await firstResponse.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken
        );

        PaginatedResponseWrapper? firstPage = JsonSerializer.Deserialize<PaginatedResponseWrapper>(
            firstResponseContent,
            JsonOptions
        );

        firstPage.ShouldNotBeNull();
        firstPage.Links.Next.ShouldNotBeNull("First page should have a next link");

        // Debug the Next URL
        string nextUrl = firstPage.Links.Next.Href;

        // Extract continuation token from the next link URL using more robust parsing
        NameValueCollection queryParams = System.Web.HttpUtility.ParseQueryString(
            new Uri(nextUrl).Query
        );
        string continuationToken =
            queryParams["continuationtoken"] ?? queryParams["continuationToken"] ?? "";

        continuationToken.ShouldNotBeNullOrEmpty(
            "Should be able to extract continuation token from Next link"
        );

        HttpResponseMessage secondResponse = await _client.GetAsync(
            GetEventsEndpoint(
                $"iTwinId={targetITwinId}&type={eventType}&top={pageSize}&continuationtoken={continuationToken}"
            ),
            TestContext.Current.CancellationToken
        );

        secondResponse.EnsureSuccessStatusCode();
        string secondResponseContent = await secondResponse.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken
        );
        PaginatedResponseWrapper? secondPage = JsonSerializer.Deserialize<PaginatedResponseWrapper>(
            secondResponseContent,
            JsonOptions
        );

        secondPage.ShouldNotBeNull();
        secondPage.Items.ShouldNotBeNull();

        // Verify all events match the filter criteria
        foreach (Event evt in secondPage.Items)
        {
            evt.ITwinId.ShouldBe(targetITwinId);
            evt.Type.ShouldBe(eventType);
        }
    }
}
