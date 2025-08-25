// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Kumara.TestCommon.Helpers;

public static class ModelHelpers
{
    public static string?[] ValidateModel(object model)
    {
        var context = new ValidationContext(model, null, null);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, true);
        return results.Select(result => result.ErrorMessage).ToArray();
    }
}
