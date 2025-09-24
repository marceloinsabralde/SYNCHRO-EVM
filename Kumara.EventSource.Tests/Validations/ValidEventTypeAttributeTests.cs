// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Kumara.EventSource.Validations;

namespace Kumara.EventSource.Tests.Validations;

public sealed class ValidEventTypeAttributeTests
{
    private readonly ValidationContext _validationContext = new ValidationContext(new object())
    {
        DisplayName = "Test",
    };

    [Theory]
    [InlineData("activity.created.v1")]
    [InlineData("activity.updated.v1")]
    [InlineData("activity.deleted.v1")]
    [InlineData("controlaccount.created.v1")]
    [InlineData("controlaccount.updated.v1")]
    [InlineData("controlaccount.deleted.v1")]
    public void ValidationSucceeds(object input)
    {
        var attr = new ValidEventTypeAttribute();
        var result = attr.GetValidationResult(input, _validationContext)!;
        result.ShouldBe(ValidationResult.Success);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ActivityCreatedV1")]
    [InlineData("activity-created-v1")]
    public void ValidationFails(object input)
    {
        var attr = new ValidEventTypeAttribute();
        var result = attr.GetValidationResult(input, _validationContext);
        result.ShouldNotBeNull();
        result.ShouldNotBe(ValidationResult.Success);
        result.ErrorMessage.ShouldBe($"\"{input}\" is not a valid Event Type.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(true)]
    [InlineData(false)]
    public void UnsupportedTypes_ThrowException(object input)
    {
        var attr = new ValidEventTypeAttribute();
        Should.Throw<ArgumentException>(() => attr.GetValidationResult(input, _validationContext));
    }
}
