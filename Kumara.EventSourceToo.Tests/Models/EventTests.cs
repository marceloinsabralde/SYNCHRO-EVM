// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSourceToo.Tests.Factories;

namespace Kumara.EventSourceToo.Tests.Models;

public class EventTests : DatabaseTestBase
{
    [Fact]
    internal async Task GeneratesIdOnPersistence()
    {
        var @event = EventFactory.CreateActivityCreatedV1Event(eventId: Guid.Empty);

        // Initialized with default value
        @event.Id.ShouldBe(Guid.Empty);

        await _dbContext.Events.AddAsync(@event, TestContext.Current.CancellationToken);

        @event.Id.ShouldNotBe(Guid.Empty);
    }
}
