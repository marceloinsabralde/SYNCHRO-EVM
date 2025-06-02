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
        result.ITwinId.ShouldNotBe(Guid.Empty);
        result.AccountId.ShouldNotBe(Guid.Empty);
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
        result.ITwinId.ShouldBe(Guid.Empty);
        result.AccountId.ShouldBe(Guid.Empty);
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
        Guid customITwinId = Guid.NewGuid();
        string customCorrelationId = "custom-correlation-id";

        // Act
        Event result = _factory.WithCustomProperties(evt =>
        {
            evt.ITwinId = customITwinId;
            evt.CorrelationId = customCorrelationId;
        });

        // Assert
        result.ShouldNotBeNull();
        result.ITwinId.ShouldBe(customITwinId);
        result.CorrelationId.ShouldBe(customCorrelationId);
    }
}
