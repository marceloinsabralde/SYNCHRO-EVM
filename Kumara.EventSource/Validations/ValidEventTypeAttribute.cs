// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Kumara.Common.Utilities;

namespace Kumara.EventSource.Validations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class ValidEventTypeAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        return value switch
        {
            null => ValidationResult.Success,
            string eventTypeName => GetValidationResult(eventTypeName),
            _ => throw new ArgumentException(
                $"{nameof(ValidEventTypeAttribute)} does not support {value.GetType().Name} types",
                nameof(value)
            ),
        };
    }

    private static ValidationResult? GetValidationResult(string eventTypeName)
    {
        if (EventTypeRegistry.Instance.IsValidEventType(eventTypeName))
            return ValidationResult.Success;
        return new ValidationResult($"\"{eventTypeName}\" is not a valid Event Type.");
    }
}
