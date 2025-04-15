// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using CloudNative.CloudEvents;

namespace Kumara.EventSource.Interfaces;

public interface IEventRepository
{
    Task<IQueryable<CloudEvent>> GetAllEventsAsync();
    Task AddEventsAsync(IEnumerable<CloudEvent> cloudEvents);
}
