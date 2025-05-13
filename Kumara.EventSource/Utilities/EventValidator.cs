// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Models;

namespace Kumara.EventSource.Utilities;

public class EventValidator(Dictionary<string, Type> eventTypeMap) : IEventValidator
{
    private readonly Dictionary<string, Type> _eventTypeMap = eventTypeMap;

    public ValidationResult ValidateEvent(Event @event)
    {
        string eventType = @event.Type;
        JsonDocument eventData = @event.DataJson;

        if (!_eventTypeMap.ContainsKey(eventType))
        {
            return ValidationResult.Failure($"The event type '{eventType}' is not supported.");
        }

        Type? eventTypeInstance = GetEventType(_eventTypeMap, eventType);
        if (eventTypeInstance != null)
        {
            try
            {
                object? data = JsonSerializer.Deserialize(eventData, eventTypeInstance);

                if (data == null)
                {
                    return ValidationResult.Failure(
                        $"The data for event type '{eventType}' is invalid."
                    );
                }

                List<System.ComponentModel.DataAnnotations.ValidationResult> validationResults =
                    new();
                bool isValid = Validator.TryValidateObject(
                    data,
                    new ValidationContext(data),
                    validationResults,
                    true
                );

                if (!isValid)
                {
                    return ValidationResult.Failure(
                        validationResults.Select(vr => vr.ErrorMessage ?? "Unknown error").ToList()
                    );
                }
            }
            catch (JsonException ex)
            {
                return ValidationResult.Failure($"Error deserializing JSON: {ex.Message}");
            }
        }

        return ValidationResult.Success();
    }

    private static Type? GetEventType(Dictionary<string, Type> eventTypeMap, string eventType)
    {
        eventTypeMap.TryGetValue(eventType, out Type? type);
        return type;
    }
}

public class ValidationResult
{
    public bool IsValid { get; private set; }
    public IReadOnlyList<string> Errors { get; private set; }

    private ValidationResult(bool isValid, IReadOnlyList<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    public static ValidationResult Success()
    {
        return new ValidationResult(true, []);
    }

    public static ValidationResult Failure(string error)
    {
        return new ValidationResult(false, [error]);
    }

    public static ValidationResult Failure(IEnumerable<string> errors)
    {
        return new ValidationResult(false, [.. errors]);
    }
}
