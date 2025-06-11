// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Kumara.WebApi.Validations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class NotEmptyAttribute() : ValidationAttribute(DefaultErrorMessage)
{
    private const string DefaultErrorMessage = "{0} must not be empty.";

    public override bool IsValid(object? value)
    {
        return value switch
        {
            null => true,
            string str => str != String.Empty,
            Guid guid => guid != Guid.Empty,
            _ => throw new ArgumentException(
                $"{nameof(NotEmptyAttribute)} does not support {value.GetType().Name} types",
                nameof(value)
            ),
        };
    }
}
