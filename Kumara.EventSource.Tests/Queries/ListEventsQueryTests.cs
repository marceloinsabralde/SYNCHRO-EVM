// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.Models;
using Kumara.EventSource.Queries;
using Kumara.EventSource.Tests.Factories;

namespace Kumara.EventSource.Tests.Queries;

public class ListEventsQueryTests : DatabaseTestBase
{
    public static Guid ITwinId = Guid.CreateVersion7();
    public static Guid AccountId = Guid.CreateVersion7();
    public static List<Event> Events = Enumerable
        .Range(0, 10)
        .Select(index =>
        {
            var timestamp = DateTimeOffset.UtcNow.AddDays(-index);
            return EventFactory.CreateActivityCreatedV1Event(
                eventId: Guid.CreateVersion7(timestamp),
                accountId: AccountId,
                iTwinId: ITwinId
            );
        })
        .OrderBy(@event => @event.Id)
        .ToList();

    public static Event OtherITwinEvent = EventFactory.CreateActivityCreatedV1Event(
        iTwinId: Guid.CreateVersion7(),
        accountId: AccountId
    );

    public static Event OtherAccountEvent = EventFactory.CreateActivityCreatedV1Event();

    public static IEnumerable<Event> AllEvents =>
        Events.Concat([OtherITwinEvent, OtherAccountEvent]);

    internal async Task Setup(IEnumerable<Event>? events = null)
    {
        if (events is null)
            events = AllEvents;

        await _dbContext.Events.AddRangeAsync(events);
        await _dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task EmptyFilter_Test()
    {
        await Setup();

        var queryResult = new ListEventsQuery(_dbContext.Events.AsQueryable())
            .ApplyFilter(new ListEventsQueryFilter())
            .ExecuteQuery();

        queryResult.Items.ShouldBe(AllEvents);
    }

    [Fact]
    public async Task ContinueFromId_Test()
    {
        await Setup();
        var continueFromId = Events.ElementAt(4).Id;

        var queryResult = new ListEventsQuery(query: _dbContext.Events.AsQueryable())
            .ApplyFilter(new() { ContinueFromId = continueFromId })
            .ExecuteQuery();

        queryResult.Items.ShouldBe(AllEvents.ToList().GetRange(5, 7));
    }

    [Fact]
    public async Task ITwinId_Test()
    {
        await Setup();

        var queryResult = new ListEventsQuery(query: _dbContext.Events.AsQueryable())
            .ApplyFilter(new() { ITwinId = ITwinId })
            .ExecuteQuery();

        queryResult.Items.ShouldBe(Events);
    }

    [Fact]
    public async Task ITwinIdWithContinueFromId_Test()
    {
        await Setup();
        var continueFromId = Events.ElementAt(4).Id;

        var queryResult = new ListEventsQuery(query: _dbContext.Events.AsQueryable())
            .ApplyFilter(new() { ITwinId = ITwinId, ContinueFromId = continueFromId })
            .ExecuteQuery();

        queryResult.Items.ShouldBe(Events.GetRange(5, 5));
    }

    [Fact]
    public async Task AccountId_Test()
    {
        await Setup();

        var queryResult = new ListEventsQuery(query: _dbContext.Events.AsQueryable())
            .ApplyFilter(new() { AccountId = AccountId })
            .ExecuteQuery();

        queryResult.Items.ShouldBe(Events.Concat([OtherITwinEvent]));
    }

    [Fact]
    public async Task EventType_Test()
    {
        var deletedActivityEvent = EventFactory.CreateActivityDeletedV1Event();
        await Setup(AllEvents.Append(deletedActivityEvent));

        var queryResult = new ListEventsQuery(query: _dbContext.Events.AsQueryable())
            .ApplyFilter(new() { EventType = "activity.deleted.v1" })
            .ExecuteQuery();

        queryResult.Items.ShouldBe(new List<Event>() { deletedActivityEvent });
    }
}
