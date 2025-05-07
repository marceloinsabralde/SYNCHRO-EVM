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
    public void ValidateEventEntity_ValidTestCreatedV1Event_ReturnsTrue()
    {
        EventEntity eventEntity = new()
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

        ValidationResult result = SEventValidator.ValidateEvent(eventEntity);

        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [TestMethod]
    public void ValidateEventEntity_InvalidTestCreatedV1Event_ReturnsFalse()
    {
        EventEntity eventEntity = new()
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

        ValidationResult result = SEventValidator.ValidateEvent(eventEntity);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors[0].ShouldContain("The field TestInteger must be between 0 and 1000.");
    }

    [TestMethod]
    public void ValidateEventEntity_InvalidDataJson_ReturnsFalse()
    {
        EventEntity eventEntity = new()
        {
            ITwinGuid = Guid.NewGuid(),
            AccountGuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString(),
            SpecVersion = "1.0",
            Source = new Uri("http://example.com/TestSource"),
            Type = "test.created.v1",
            DataJson = JsonDocument.Parse("{}"),
        };

        ValidationResult result = SEventValidator.ValidateEvent(eventEntity);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors[0].ShouldContain("Error deserializing JSON");
    }

    [TestMethod]
    public void ValidateEventEntity_ValidTestUpdatedV1Event_ReturnsTrue()
    {
        // Arrange
        var eventEntity = new EventEntity
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

        // Act
        var result = SEventValidator.ValidateEvent(eventEntity);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [TestMethod]
    public void ValidateEventEntity_InvalidTestUpdatedV1Event_ReturnsFalse()
    {
        // Arrange
        var eventEntity = new EventEntity
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

        // Act
        var result = SEventValidator.ValidateEvent(eventEntity);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors[0].ShouldContain("The field TestInteger must be between 0 and 1000.");
    }
}
