// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSource.Models;

namespace Kumara.EventSource.Tests.Factories;

/// <summary>
/// Base factory for creating test events with common properties.
/// </summary>
/// <typeparam name="TData">The type of the event data.</typeparam>
public abstract class EventFactory<TData>
    where TData : class
{
    private static readonly Uri TestSource = new("http://testsource.com");

    /// <summary>
    /// Creates a valid Event with the specified data.
    /// </summary>
    /// <param name="data">The event data to include.</param>
    /// <returns>A valid Event instance.</returns>
    protected Event CreateEventWithData(TData data)
    {
        return new Event
        {
            ITwinId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = TestSource,
            Type = GetEventType(),
            DataJson = JsonSerializer.SerializeToDocument(data),
        };
    }

    /// <summary>
    /// Gets the event type string for this factory.
    /// </summary>
    /// <returns>The event type string.</returns>
    protected abstract string GetEventType();
}
