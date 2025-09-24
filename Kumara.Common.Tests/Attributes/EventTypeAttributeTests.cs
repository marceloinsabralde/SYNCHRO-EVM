// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Reflection;
using Kumara.Common.Attributes;

namespace Kumara.Common.Tests.Attributes;

public class EventTypeAttributeTests
{
    [EventType("test")]
    class TestClassWithEventTypeAttribute;

    class TestClassWithoutEventTypeAttribute;

    [Fact]
    public void AttributeHasName()
    {
        var result = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Select(type => type.GetCustomAttribute<EventTypeAttribute>())
            .Where(attr => attr is not null)
            .ToArray();

        EventTypeAttribute[] expected = [new("test")];
        result.ShouldBeEquivalentTo(expected);
    }
}
