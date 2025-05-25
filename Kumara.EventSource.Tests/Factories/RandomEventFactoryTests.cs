// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSource.Models;

namespace Kumara.EventSource.Tests.Factories;

public class RandomEventFactoryTests
{
    private readonly IEventFactory<Event> _factory = new RandomEventFactory();

    [Fact]
    public void CreateValid_ReturnsEventWithValidData()
    {
        // Act
        Event result = _factory.CreateValid();

        // Assert
        result.ShouldNotBeNull();
        result.ITwinGuid.ShouldNotBe(Guid.Empty);
        result.AccountGuid.ShouldNotBe(Guid.Empty);
        result.CorrelationId.ShouldNotBeEmpty();
        result.SpecVersion.ShouldBe("1.0");
        result.Source.ShouldBe(new Uri("http://testsource.com"));
        result.Type.ShouldBe("some.random.event");
        result.DataJson.ShouldNotBeNull();

        JsonElement data = JsonSerializer.Deserialize<JsonElement>(result.DataJson);
        data.GetProperty("RandomString").GetString().ShouldBe("RandomValue");
        data.GetProperty("RandomNumber").GetInt32().ShouldBe(42);
        data.GetProperty("NestedObject")
            .GetProperty("NestedKey")
            .GetString()
            .ShouldBe("NestedValue");
        data.GetProperty("NestedObject")
            .GetProperty("NestedArray")
            .EnumerateArray()
            .Count()
            .ShouldBe(3);
    }

    [Fact]
    public void CreateInvalid_ReturnsEventWithInvalidData()
    {
        // Act
        Event result = _factory.CreateInvalid();

        // Assert
        result.ShouldNotBeNull();
        result.ITwinGuid.ShouldBe(Guid.Empty);
        result.AccountGuid.ShouldBe(Guid.Empty);
        result.CorrelationId.ShouldBeEmpty();
        result.SpecVersion.ShouldBe("0.0");
        result.Type.ShouldBe("some.random.event");
        result.DataJson.ShouldNotBeNull();

        JsonElement data = JsonSerializer.Deserialize<JsonElement>(result.DataJson);
        data.GetProperty("RandomString").GetString().ShouldBeEmpty();
        data.GetProperty("RandomNumber").GetInt32().ShouldBe(0);
    }

    [Fact]
    public void WithCustomProperties_AllowsCustomization()
    {
        // Arrange
        Guid customITwinGuid = Guid.NewGuid();
        string customCorrelationId = "custom-correlation-id";

        // Act
        Event result = _factory.WithCustomProperties(evt =>
        {
            evt.ITwinGuid = customITwinGuid;
            evt.CorrelationId = customCorrelationId;
        });

        // Assert
        result.ShouldNotBeNull();
        result.ITwinGuid.ShouldBe(customITwinGuid);
        result.CorrelationId.ShouldBe(customCorrelationId);
    }
}
