// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Utilities;

namespace Kumara.Common.Tests.Utilities;

public sealed class EventTypeRegistryTests
{
    [Fact]
    public void IsValidEventType_WhenValid_ReturnsTrue()
    {
        EventTypeRegistry.Instance.IsValidEventType("activity.created.v1").ShouldBeTrue();
    }

    [Fact]
    public void IsValidEventType_WhenInvalid_ReturnsFalse()
    {
        EventTypeRegistry.Instance.IsValidEventType("unknown.event.type").ShouldBeFalse();
    }

    [Fact]
    public void TryGetEventType_WhenValid_ReturnsEventType()
    {
        var result = EventTypeRegistry.Instance.TryGetEventType(
            "activity.created.v1",
            out Type? eventType
        );

        result.ShouldBeTrue();
        eventType.ShouldNotBeNull();
        eventType.Name.ShouldBe("ActivityCreatedV1");
    }

    [Fact]
    public void TryGetEventType_WhenInvalid_ReturnsNull()
    {
        var result = EventTypeRegistry.Instance.TryGetEventType(
            "unknown.event.type",
            out Type? eventType
        );

        result.ShouldBeFalse();
        eventType.ShouldBeNull();
    }

    [Fact]
    public void GetEventTypes_ReturnsAllEventTypes()
    {
        EventTypeRegistry
            .Instance.GetEventTypes()
            .ToList()
            .ShouldBeEquivalentTo(
                new List<string>
                {
                    "activity.created.v1",
                    "activity.deleted.v1",
                    "activity.updated.v1",
                    "controlaccount.created.v1",
                    "controlaccount.deleted.v1",
                    "controlaccount.updated.v1",
                }
            );
    }
}
