// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Models;
using Kumara.EventSource.Models.Events;
using Kumara.EventSource.Utilities;
using Shouldly;

namespace Kumara.Tests.EventSource;

[TestClass]
public class EventValidatorTests
{
    private static readonly IEventValidator SEventValidator = new EventValidator(
        new Dictionary<string, Type>
        {
            { "test.created.v1", typeof(TestCreatedV1) },
            { "test.updated.v1", typeof(TestUpdatedV1) },
        }
    );

    [TestMethod]
    public void ValidateEvent_ValidTestCreatedV1Event_ReturnsTrue()
    {
        Event @event = new()
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "test.created.v1",
            DataJson = JsonSerializer.SerializeToDocument(
                new TestCreatedV1
                {
                    TestString = "Valid Test String",
                    TestEnum = TestOptions.OptionA,
                    TestInteger = 100,
                }
            ),
        };

        ValidationResult result = SEventValidator.ValidateEvent(@event);

        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [TestMethod]
    public void ValidateEvent_InvalidTestCreatedV1Event_ReturnsFalse()
    {
        Event @event = new()
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "test.created.v1",
            DataJson = JsonSerializer.SerializeToDocument(
                new TestCreatedV1
                {
                    TestString = "Test String",
                    TestEnum = TestOptions.OptionA,
                    TestInteger = 2000,
                }
            ),
        };

        ValidationResult result = SEventValidator.ValidateEvent(@event);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors[0].ShouldContain("The field TestInteger must be between 0 and 1000.");
    }

    [TestMethod]
    public void ValidateEvent_InvalidDataJson_ReturnsFalse()
    {
        Event @event = new()
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "test.created.v1",
            DataJson = JsonDocument.Parse("{}"),
        };

        ValidationResult result = SEventValidator.ValidateEvent(@event);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors[0].ShouldContain("Error deserializing JSON");
    }

    [TestMethod]
    public void ValidateEvent_ValidTestUpdatedV1Event_ReturnsTrue()
    {
        Event @event = new()
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "test.updated.v1",
            DataJson = JsonSerializer.SerializeToDocument(
                new TestUpdatedV1
                {
                    TestString = "Valid Updated Test String",
                    TestEnum = TestOptions.OptionB,
                    TestInteger = 200,
                    UpdatedTime = DateTime.UtcNow,
                }
            ),
        };

        ValidationResult result = SEventValidator.ValidateEvent(@event);

        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [TestMethod]
    public void ValidateEvent_InvalidTestUpdatedV1Event_ReturnsFalse()
    {
        Event @event = new()
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "test.updated.v1",
            DataJson = JsonSerializer.SerializeToDocument(
                new TestUpdatedV1
                {
                    TestString = "Test String",
                    TestEnum = TestOptions.OptionC,
                    TestInteger = 2000, // Invalid: outside range
                    UpdatedTime = DateTime.UtcNow,
                }
            ),
        };

        ValidationResult result = SEventValidator.ValidateEvent(@event);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors[0].ShouldContain("The field TestInteger must be between 0 and 1000.");
    }
}
