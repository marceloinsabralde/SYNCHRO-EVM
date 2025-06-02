// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Kumara.EventSource.Models.Events;
using Kumara.EventSource.Tests.Common;
using Kumara.EventSource.Tests.Factories;
using Shouldly;

namespace Kumara.EventSource.Tests.Models;

public class ActivityEventModelsTests
{
    [Fact]
    public void ActivityCreatedV1_WithReferenceCode_IsValid()
    {
        DateTimeOffset now = CommonTestUtilities.GetTestDateTimeOffset();
        ActivityCreatedV1 model = ActivityCreatedV1Factory.Create(
            referenceCode: "ACT-123",
            plannedStart: now,
            plannedFinish: now.AddDays(10)
        );
        List<ValidationResult> validationResults = new();
        bool isValid = Validator.TryValidateObject(
            model,
            new ValidationContext(model),
            validationResults,
            true
        );
        isValid.ShouldBeTrue();
        validationResults.ShouldBeEmpty();
    }

    [Fact]
    public void ActivityCreatedV1_WithEmptyReferenceCode_IsInvalid()
    {
        DateTimeOffset now = CommonTestUtilities.GetTestDateTimeOffset();
        // We need to provide an empty string for ReferenceCode since it's a required property
        ActivityCreatedV1 model = new()
        {
            Id = Guid.NewGuid(),
            Name = "Test Activity",
            ReferenceCode = string.Empty, // Empty string will fail validation but compile
            ControlAccountId = Guid.NewGuid(),
            PlannedStart = now,
            PlannedFinish = now.AddDays(10),
        };
        List<ValidationResult> validationResults = new();
        bool isValid = Validator.TryValidateObject(
            model,
            new ValidationContext(model),
            validationResults,
            true
        );
        isValid.ShouldBeFalse();
        validationResults.ShouldNotBeEmpty();
        validationResults[0].ErrorMessage?.ShouldContain("ReferenceCode");
    }

    [Fact]
    public void ActivityUpdatedV1_WithReferenceCode_IsValid()
    {
        DateTimeOffset now = CommonTestUtilities.GetTestDateTimeOffset();
        ActivityUpdatedV1 model = ActivityUpdatedV1Factory.Create(
            referenceCode: "ACT-123",
            plannedStart: now,
            plannedFinish: now.AddDays(10),
            actualStart: now,
            actualFinish: now.AddDays(9)
        );
        List<ValidationResult> validationResults = new();
        bool isValid = Validator.TryValidateObject(
            model,
            new ValidationContext(model),
            validationResults,
            true
        );
        isValid.ShouldBeTrue();
        validationResults.ShouldBeEmpty();
    }

    [Fact]
    public void ActivityUpdatedV1_WithEmptyReferenceCode_IsInvalid()
    {
        DateTimeOffset now = CommonTestUtilities.GetTestDateTimeOffset();
        ActivityUpdatedV1 model = new()
        {
            Id = Guid.NewGuid(),
            Name = "Test Activity",
            ReferenceCode = string.Empty, // Empty string will fail validation but compile
            ControlAccountId = Guid.NewGuid(),
            PlannedStart = now,
            PlannedFinish = now.AddDays(10),
            ActualStart = now,
            ActualFinish = now.AddDays(9),
        };
        List<ValidationResult> validationResults = new();
        bool isValid = Validator.TryValidateObject(
            model,
            new ValidationContext(model),
            validationResults,
            true
        );
        isValid.ShouldBeFalse();
        validationResults.ShouldNotBeEmpty();
        validationResults[0].ErrorMessage?.ShouldContain("ReferenceCode");
    }
}
