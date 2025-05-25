// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

namespace Kumara.EventSource.Utilities;

public class EventValidationResult
{
    public bool IsValid { get; private set; }

    public IReadOnlyList<string> Errors { get; private set; }

    private EventValidationResult(bool isValid, IReadOnlyList<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    public static EventValidationResult Success()
    {
        return new EventValidationResult(true, []);
    }

    public static EventValidationResult Failure(string error)
    {
        return new EventValidationResult(false, [error]);
    }

    public static EventValidationResult Failure(IEnumerable<string> errors)
    {
        return new EventValidationResult(false, [.. errors]);
    }
}
