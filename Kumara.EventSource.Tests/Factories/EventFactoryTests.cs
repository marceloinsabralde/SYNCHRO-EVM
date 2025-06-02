// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSource.Models;
using Kumara.EventSource.Models.Events;

namespace Kumara.EventSource.Tests.Factories;

public class EventFactoryTests
{
    private readonly IEventFactory<Event> _factory = new ControlAccountCreatedV1Factory();

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
        result.Type.ShouldBe("control.account.created.v1");
        result.DataJson.ShouldNotBeNull();

        ControlAccountCreatedV1? data = JsonSerializer.Deserialize<ControlAccountCreatedV1>(
            result.DataJson
        );
        data.ShouldNotBeNull();
        data.Id.ShouldNotBe(Guid.Empty);
        data.Name.ShouldBe("Test Account");
        data.WbsPath.ShouldBe("1.2.3");
        data.TaskId.ShouldNotBe(Guid.Empty);
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
        result.Type.ShouldBe("control.account.created.v1");
        result.DataJson.ShouldNotBeNull();

        ControlAccountCreatedV1? data = JsonSerializer.Deserialize<ControlAccountCreatedV1>(
            result.DataJson
        );
        data.ShouldNotBeNull();
        data.Id.ShouldBe(Guid.Empty);
        data.Name.ShouldBeNull();
        data.WbsPath.ShouldBeEmpty();
        data.TaskId.ShouldBe(Guid.Empty);
        data.PlannedStart.ShouldBe(DateTimeOffset.MinValue);
        data.PlannedFinish.ShouldBe(DateTimeOffset.MaxValue);
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
