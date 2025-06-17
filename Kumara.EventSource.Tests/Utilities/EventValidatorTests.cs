// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Models;
using Kumara.EventSource.Models.Events;
using Kumara.EventSource.Tests.Common;
using Kumara.EventSource.Utilities;

namespace Kumara.EventSource.Tests.Utilities;

public class EventValidatorTests
{
    private static readonly IEventValidator SEventValidator = new EventValidator(
        new Dictionary<string, Type>
        {
            { "control.account.created.v1", typeof(ControlAccountCreatedV1) },
            { "control.account.updated.v1", typeof(ControlAccountUpdatedV1) },
        }
    );

    private static readonly IEventValidator SActivityEventValidator = new EventValidator(
        new Dictionary<string, Type>
        {
            { "activity.created.v1", typeof(ActivityCreatedV1) },
            { "activity.updated.v1", typeof(ActivityUpdatedV1) },
        }
    );

    [Fact]
    public async Task ValidateEventAsync_ValidControlAccountCreatedV1Event_ReturnsTrue()
    {
        DateTimeOffset now = CommonTestUtilities.GetTestDateTimeOffset();

        Event @event = new()
        {
            ITwinId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("https://example.com/TestSource"),
            Type = "control.account.created.v1",
            Time = now,
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

        EventValidationResult result = await SEventValidator.ValidateEventAsync(
            @event,
            TestContext.Current.CancellationToken
        );

        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public async Task ValidateEventAsync_InvalidControlAccountCreatedV1Event_ReturnsFalse()
    {
        Event @event = new()
        {
            ITwinId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
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

        EventValidationResult result = await SEventValidator.ValidateEventAsync(
            @event,
            TestContext.Current.CancellationToken
        );

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors[0].ShouldContain("missing required properties");
    }

    [Fact]
    public async Task ValidateEventAsync_InvalidDataJson_ReturnsFalse()
    {
        Event @event = new()
        {
            ITwinId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("https://example.com/TestSource"),
            Type = "control.account.created.v1",
            DataJson = JsonDocument.Parse("{}"),
        };

        EventValidationResult result = await SEventValidator.ValidateEventAsync(
            @event,
            TestContext.Current.CancellationToken
        );

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors[0].ShouldContain("missing required properties");
    }

    [Fact]
    public async Task ValidateEventAsync_ValidControlAccountUpdatedV1Event_ReturnsTrue()
    {
        DateTimeOffset now = CommonTestUtilities.GetTestDateTimeOffset();

        Event @event = new()
        {
            ITwinId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("https://example.com/TestSource"),
            Type = "control.account.updated.v1",
            Time = now,
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

        EventValidationResult result = await SEventValidator.ValidateEventAsync(
            @event,
            TestContext.Current.CancellationToken
        );

        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public async Task ValidateEventAsync_InvalidControlAccountUpdatedV1Event_ReturnsFalse()
    {
        Event @event = new()
        {
            ITwinId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
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

        EventValidationResult result = await SEventValidator.ValidateEventAsync(
            @event,
            TestContext.Current.CancellationToken
        );

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors[0].ShouldContain("The Name field is required.");
    }

    [Fact]
    public async Task ValidateEventAsync_ValidActivityCreatedV1Event_ReturnsTrue()
    {
        DateTimeOffset now = CommonTestUtilities.GetTestDateTimeOffset();

        Event @event = new()
        {
            ITwinId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("https://example.com/TestSource"),
            Type = "activity.created.v1",
            Time = now,
            DataJson = JsonSerializer.SerializeToDocument(
                new ActivityCreatedV1
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Activity",
                    ReferenceCode = "ACT-123",
                    ControlAccountId = Guid.NewGuid(),
                    PlannedStart = now,
                    PlannedFinish = now.AddDays(10),
                    ActualStart = now,
                    ActualFinish = now.AddDays(9),
                }
            ),
        };

        EventValidationResult result = await SActivityEventValidator.ValidateEventAsync(
            @event,
            TestContext.Current.CancellationToken
        );

        result.IsValid.ShouldBeTrue();
        if (result.Errors != null)
        {
            result.Errors.ShouldBeEmpty();
        }
    }

    [Fact]
    public async Task ValidateEventAsync_ActivityCreatedV1MissingRequiredFields_ReturnsFalse()
    {
        Event @event = new()
        {
            ITwinId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("https://example.com/TestSource"),
            Type = "activity.created.v1",
            DataJson = JsonSerializer.SerializeToDocument(
                new ActivityCreatedV1
                {
                    Id = Guid.Empty,
                    Name = "",
                    ReferenceCode = "",
                    ControlAccountId = Guid.Empty,
                }
            ),
        };

        EventValidationResult result = await SActivityEventValidator.ValidateEventAsync(
            @event,
            TestContext.Current.CancellationToken
        );

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain(e => e.Contains("The Name field is required"));
        result.Errors.ShouldContain(e => e.Contains("The ReferenceCode field is required"));
    }

    [Fact]
    public async Task ValidateEventAsync_ValidActivityUpdatedV1Event_ReturnsTrue()
    {
        DateTimeOffset now = CommonTestUtilities.GetTestDateTimeOffset();

        Event @event = new()
        {
            ITwinId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("https://example.com/TestSource"),
            Type = "activity.updated.v1",
            Time = now,
            DataJson = JsonSerializer.SerializeToDocument(
                new ActivityUpdatedV1
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Activity",
                    ReferenceCode = "ACT-123",
                    ControlAccountId = Guid.NewGuid(),
                    PlannedStart = now,
                    PlannedFinish = now.AddDays(10),
                    ActualStart = now,
                    ActualFinish = now.AddDays(9),
                }
            ),
        };

        EventValidationResult result = await SActivityEventValidator.ValidateEventAsync(
            @event,
            TestContext.Current.CancellationToken
        );

        result.IsValid.ShouldBeTrue();
        if (result.Errors != null)
        {
            result.Errors.ShouldBeEmpty();
        }
    }

    [Fact]
    public async Task ValidateEventAsync_ActivityUpdatedV1MissingRequiredFields_ReturnsFalse()
    {
        Event @event = new()
        {
            ITwinId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("https://example.com/TestSource"),
            Type = "activity.updated.v1",
            DataJson = JsonSerializer.SerializeToDocument(
                new ActivityUpdatedV1
                {
                    Id = Guid.Empty,
                    Name = "",
                    ReferenceCode = "",
                    ControlAccountId = Guid.Empty,
                }
            ),
        };

        EventValidationResult result = await SActivityEventValidator.ValidateEventAsync(
            @event,
            TestContext.Current.CancellationToken
        );

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain(e => e.Contains("The Name field is required"));
        result.Errors.ShouldContain(e => e.Contains("The ReferenceCode field is required"));
    }
}
