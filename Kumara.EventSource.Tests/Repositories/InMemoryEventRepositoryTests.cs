// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Repositories;

namespace Kumara.EventSource.Tests.Repositories;

public class InMemoryEventRepositoryTests : EventRepositoryTestsBase
{
    private readonly IEventRepository _eventRepository = null!;

    protected override IEventRepository EventRepository => _eventRepository;

    // Constructor for setup logic
    public InMemoryEventRepositoryTests()
    {
        _eventRepository = new EventRepositoryInMemoryList();
    }

    // No need to implement the common test methods as they are inherited from the base class
    // Add any specialized tests specific to InMemoryEventRepository here
}
