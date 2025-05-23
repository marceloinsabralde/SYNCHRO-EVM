// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Models;
using Kumara.EventSource.Models.Events;
using Kumara.EventSource.Utilities;
using Shouldly;

namespace Kumara.EventSource.Tests;

[TestClass]
public class EventValidatorTests
{
    private static readonly IEventValidator SEventValidator = new EventValidator(
        new Dictionary<string, Type>
        {
            { "control.account.created.v1", typeof(ControlAccountCreatedV1) },
            { "control.account.updated.v1", typeof(ControlAccountUpdatedV1) },
        }
    );

    [TestMethod]
    public async Task ValidateEventAsync_ValidControlAccountCreatedV1Event_ReturnsTrue()
    {
        DateTimeOffset now = EventRepositoryTestUtils.GetTestDateTimeOffset();

        Event @event = new()
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("https://example.com/TestSource"),
            Type = "control.account.created.v1",
            DataJson = JsonSerializer.SerializeToDocument(
                new ControlAccountCreatedV1
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Account",
                    WbsPath = "1.2.3",
                    TaskId = Guid.NewGuid(),
                    PlannedStart = now,
                    PlannedFinish = now.AddDays(10),
                    ActualStart = now,
                    ActualFinish = now.AddDays(9),
                }
            ),
        };

        EventValidationResult result = await SEventValidator.ValidateEventAsync(@event);

        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task ValidateEventAsync_InvalidControlAccountCreatedV1Event_ReturnsFalse()
    {
        Event @event = new()
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("https://example.com/TestSource"),
            Type = "control.account.created.v1",
            DataJson = JsonSerializer.SerializeToDocument(
                new
                {
                    Name = "Hello World",
                    WbsPath = "",
                    TaskId = Guid.NewGuid(),
                }
            ),
        };

        EventValidationResult result = await SEventValidator.ValidateEventAsync(@event);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors[0].ShouldContain("missing required properties");
    }

    [TestMethod]
    public async Task ValidateEventAsync_InvalidDataJson_ReturnsFalse()
    {
        Event @event = new()
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("https://example.com/TestSource"),
            Type = "control.account.created.v1",
            DataJson = JsonDocument.Parse("{}"),
        };

        EventValidationResult result = await SEventValidator.ValidateEventAsync(@event);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors[0].ShouldContain("missing required properties");
    }

    [TestMethod]
    public async Task ValidateEventAsync_ValidControlAccountUpdatedV1Event_ReturnsTrue()
    {
        DateTimeOffset now = EventRepositoryTestUtils.GetTestDateTimeOffset();

        Event @event = new()
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("https://example.com/TestSource"),
            Type = "control.account.updated.v1",
            DataJson = JsonSerializer.SerializeToDocument(
                new ControlAccountUpdatedV1
                {
                    Id = Guid.NewGuid(),
                    Name = "Updated Account",
                    WbsPath = "1.2.3",
                    TaskId = Guid.NewGuid(),
                    PlannedStart = now,
                    PlannedFinish = now.AddDays(10),
                    ActualStart = now,
                    ActualFinish = now.AddDays(9),
                }
            ),
        };

        EventValidationResult result = await SEventValidator.ValidateEventAsync(@event);

        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task ValidateEventAsync_InvalidControlAccountUpdatedV1Event_ReturnsFalse()
    {
        Event @event = new()
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("https://example.com/TestSource"),
            Type = "control.account.updated.v1",
            DataJson = JsonSerializer.SerializeToDocument(
                new ControlAccountUpdatedV1
                {
                    Id = Guid.Empty,
                    Name = "",
                    WbsPath = "test-path",
                    TaskId = Guid.Empty,
                    PlannedStart = null,
                    PlannedFinish = null,
                    ActualStart = null,
                    ActualFinish = null,
                }
            ),
        };

        EventValidationResult result = await SEventValidator.ValidateEventAsync(@event);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors[0].ShouldContain("The Name field is required.");
    }
}
