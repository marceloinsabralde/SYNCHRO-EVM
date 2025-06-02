// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSource.Models;
using Kumara.EventSource.Models.Events;
using Kumara.EventSource.Tests.Common;

namespace Kumara.EventSource.Tests.Factories;

/// <summary>
/// Factory for creating test instances of ControlAccountCreatedV1 events.
/// </summary>
public class ControlAccountCreatedV1Factory : IEventFactory<Event>
{
    private static readonly DateTimeOffset TestDateTime =
        CommonTestUtilities.GetTestDateTimeOffset();

    private static readonly Uri TestSource = new("http://testsource.com");

    /// <inheritdoc/>
    public Event CreateValid()
    {
        ControlAccountCreatedV1 data = new()
        {
            Id = Guid.NewGuid(),
            Name = "Test Account",
            WbsPath = "1.2.3",
            TaskId = Guid.NewGuid(),
            PlannedStart = TestDateTime,
            PlannedFinish = TestDateTime.AddDays(10),
            ActualStart = TestDateTime,
            ActualFinish = TestDateTime.AddDays(9),
        };

        return new Event
        {
            ITwinId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = TestSource,
            Type = "control.account.created.v1",
            DataJson = JsonSerializer.SerializeToDocument(data),
        };
    }

    /// <inheritdoc/>
    public Event CreateInvalid()
    {
        ControlAccountCreatedV1 data = new()
        {
            Id = Guid.Empty,
            Name = null!,
            WbsPath = string.Empty,
            TaskId = Guid.Empty,
            PlannedStart = DateTimeOffset.MinValue,
            PlannedFinish = DateTimeOffset.MaxValue,
            ActualStart = DateTimeOffset.MaxValue,
            ActualFinish = DateTimeOffset.MinValue,
        };

        return new Event
        {
            ITwinId = Guid.Empty,
            AccountId = Guid.Empty,
            CorrelationId = string.Empty,
            SpecVersion = "0.0",
            Source = TestSource,
            Type = "control.account.created.v1",
            DataJson = JsonSerializer.SerializeToDocument(data),
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
