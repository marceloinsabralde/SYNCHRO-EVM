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
        new Dictionary<string, Type> { { "test.created.v1", typeof(TestCreatedV1) } }
    );

    [TestMethod]
    public void ValidateEventEntity_ValidEvent_ReturnsTrue()
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
    public void ValidateEventEntity_InvalidEvent_ReturnsFalse()
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
}
