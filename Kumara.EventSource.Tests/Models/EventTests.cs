// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.Tests.Factories;

namespace Kumara.EventSource.Tests.Models;

public class EventTests : ApplicationTestBase
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
