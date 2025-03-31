// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using CloudNative.CloudEvents;

using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Repositories;

using Moq;

using Shouldly;

namespace Kumara.Tests.EventSource;

[TestClass]
public class EventRepositoryTests
{
    private readonly Mock<IEventRepository> _mockEventRepository = new();

    private List<CloudEvent> GetTestCloudEvents()
    {
        return new List<CloudEvent>
        {
            new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Time = DateTimeOffset.UtcNow
            },
            new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Time = DateTimeOffset.UtcNow
            }
        };
    }

    [TestMethod]
    public async Task GetAllEventsAsync_ShouldReturnQueryableOfCloudEvents()
    {
        // Arrange
        var cloudEvents = GetTestCloudEvents().AsQueryable();

        _mockEventRepository.Setup(repo => repo.GetAllEventsAsync()).ReturnsAsync(cloudEvents);

        // Act
        var result = await _mockEventRepository.Object.GetAllEventsAsync();

        // Assert
        result.ShouldBe(cloudEvents);
    }

    [TestMethod]
    public async Task AddEventsAsync_ShouldAddCloudEvents()
    {
        // Arrange
        List<CloudEvent> cloudEvents = GetTestCloudEvents();

        _mockEventRepository.Setup(repo => repo.AddEventsAsync(cloudEvents)).Returns(Task.CompletedTask);

        // Act
        await _mockEventRepository.Object.AddEventsAsync(cloudEvents);

        // Assert
        _mockEventRepository.Verify(repo => repo.AddEventsAsync(cloudEvents), Times.Once);
    }

    [TestMethod]
    public async Task RoundtripEventsAsync_ShouldStoreAndRetrieveCloudEvents()
    {
        // Arrange
        var eventRepository = new EventRepositoryInMemoryList();
        var cloudEvents = GetTestCloudEvents();

        // Act
        await eventRepository.AddEventsAsync(cloudEvents);
        IQueryable<CloudEvent> retrievedEvents = await eventRepository.GetAllEventsAsync();

        // Assert
        retrievedEvents.ShouldBe(cloudEvents.AsQueryable(), ignoreOrder: true);
    }
}
