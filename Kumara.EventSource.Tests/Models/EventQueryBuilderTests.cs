// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSource.Models;
using Kumara.EventSource.Tests.Common;

namespace Kumara.EventSource.Tests.Models;

public class EventQueryBuilderTests
{
    [Fact]
    public void WhereTimeAfter_FiltersByTimestamp()
    {
        DateTimeOffset now = CommonTestUtilities.GetTestDateTimeOffset();
        DateTimeOffset past = now.AddHours(-1);
        DateTimeOffset future = now.AddHours(1);

        List<Event> events = new()
        {
            new Event
            {
                Id = Guid.NewGuid(),
                ITwinId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                CorrelationId = "test1",
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "test.event.time",
                Time = past,
                DataJson = JsonSerializer.SerializeToDocument(new { }),
            },
            new Event
            {
                Id = Guid.NewGuid(),
                ITwinId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                CorrelationId = "test2",
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "test.event.time",
                Time = now,
                DataJson = JsonSerializer.SerializeToDocument(new { }),
            },
            new Event
            {
                Id = Guid.NewGuid(),
                ITwinId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                CorrelationId = "test3",
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "test.event.time",
                Time = future,
                DataJson = JsonSerializer.SerializeToDocument(new { }),
            },
            new Event
            {
                Id = Guid.NewGuid(),
                ITwinId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                CorrelationId = "test4",
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "test.event.time",
                Time = null,
                DataJson = JsonSerializer.SerializeToDocument(new { }),
            },
        };
        EventQueryBuilder queryBuilder = new EventQueryBuilder().WhereTimeAfter(now);
        List<Event> results = queryBuilder.ApplyTo(events.AsQueryable()).ToList();
        results.Count.ShouldBe(2);
        results.All(e => e.Time >= now).ShouldBeTrue();
    }

    [Fact]
    public void WhereTimeBefore_FiltersByTimestamp()
    {
        DateTimeOffset now = CommonTestUtilities.GetTestDateTimeOffset();
        DateTimeOffset past = now.AddHours(-1);
        DateTimeOffset future = now.AddHours(1);

        List<Event> events = new()
        {
            new Event
            {
                Id = Guid.NewGuid(),
                ITwinId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                CorrelationId = "test1",
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "test.event.time",
                Time = past,
                DataJson = JsonSerializer.SerializeToDocument(new { }),
            },
            new Event
            {
                Id = Guid.NewGuid(),
                ITwinId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                CorrelationId = "test2",
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "test.event.time",
                Time = now,
                DataJson = JsonSerializer.SerializeToDocument(new { }),
            },
            new Event
            {
                Id = Guid.NewGuid(),
                ITwinId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                CorrelationId = "test3",
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "test.event.time",
                Time = future,
                DataJson = JsonSerializer.SerializeToDocument(new { }),
            },
            new Event
            {
                Id = Guid.NewGuid(),
                ITwinId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                CorrelationId = "test4",
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "test.event.time",
                Time = null,
                DataJson = JsonSerializer.SerializeToDocument(new { }),
            },
        };
        EventQueryBuilder queryBuilder = new EventQueryBuilder().WhereTimeBefore(now);
        List<Event> results = queryBuilder.ApplyTo(events.AsQueryable()).ToList();
        results.Count.ShouldBe(2);
        results.All(e => e.Time <= now).ShouldBeTrue();
    }

    [Fact]
    public void WhereTimeBetween_FiltersByTimestampRange()
    {
        DateTimeOffset now = CommonTestUtilities.GetTestDateTimeOffset();
        DateTimeOffset past = now.AddHours(-2);
        DateTimeOffset slightlyPast = now.AddHours(-1);
        DateTimeOffset slightlyFuture = now.AddHours(1);
        DateTimeOffset future = now.AddHours(2);

        List<Event> events = new()
        {
            new Event
            {
                Id = Guid.NewGuid(),
                ITwinId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                CorrelationId = "test1",
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "test.event.time",
                Time = past,
                DataJson = JsonSerializer.SerializeToDocument(new { }),
            },
            new Event
            {
                Id = Guid.NewGuid(),
                ITwinId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                CorrelationId = "test2",
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "test.event.time",
                Time = slightlyPast,
                DataJson = JsonSerializer.SerializeToDocument(new { }),
            },
            new Event
            {
                Id = Guid.NewGuid(),
                ITwinId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                CorrelationId = "test3",
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "test.event.time",
                Time = now,
                DataJson = JsonSerializer.SerializeToDocument(new { }),
            },
            new Event
            {
                Id = Guid.NewGuid(),
                ITwinId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                CorrelationId = "test4",
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "test.event.time",
                Time = slightlyFuture,
                DataJson = JsonSerializer.SerializeToDocument(new { }),
            },
            new Event
            {
                Id = Guid.NewGuid(),
                ITwinId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                CorrelationId = "test5",
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "test.event.time",
                Time = future,
                DataJson = JsonSerializer.SerializeToDocument(new { }),
            },
            new Event
            {
                Id = Guid.NewGuid(),
                ITwinId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                CorrelationId = "test6",
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "test.event.time",
                Time = null,
                DataJson = JsonSerializer.SerializeToDocument(new { }),
            },
        };
        EventQueryBuilder queryBuilder = new EventQueryBuilder().WhereTimeBetween(
            slightlyPast,
            slightlyFuture
        );
        List<Event> results = queryBuilder.ApplyTo(events.AsQueryable()).ToList();
        results.Count.ShouldBe(3);
        results.All(e => e.Time >= slightlyPast && e.Time <= slightlyFuture).ShouldBeTrue();
    }
}
