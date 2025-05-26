// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSource.Models;

namespace Kumara.EventSource.Tests.Factories;

/// <summary>
/// Factory for creating test instances of random events.
/// </summary>
public class RandomEventFactory : IEventFactory<Event>
{
    private static readonly Uri TestSource = new("http://testsource.com");

    /// <inheritdoc/>
    public Event CreateValid()
    {
        return new Event
        {
            ITwinId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = TestSource,
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
        };
    }

    /// <inheritdoc/>
    public Event CreateInvalid()
    {
        return new Event
        {
            ITwinId = Guid.Empty,
            AccountId = Guid.Empty,
            CorrelationId = string.Empty,
            SpecVersion = "0.0",
            Source = TestSource,
            Type = "some.random.event",
            DataJson = JsonSerializer.SerializeToDocument(
                new { RandomString = string.Empty, RandomNumber = 0 }
            ),
        };
    }

    /// <inheritdoc/>
    public Event WithCustomProperties(Action<Event> customizer)
    {
        Event instance = CreateValid();
        customizer(instance);
        return instance;
    }
}
