// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Models;

namespace Kumara.EventSource.Utilities;

public class EventValidator : IEventValidator
{
    private readonly Dictionary<string, Type> _eventTypeMap;

    public EventValidator(Dictionary<string, Type> eventTypeMap)
    {
        _eventTypeMap = eventTypeMap;
    }

    public async Task<EventValidationResult> ValidateEventAsync(
        Event @event,
        CancellationToken cancellationToken = default
    )
    {
        string eventType = @event.Type;
        JsonDocument eventData = @event.DataJson;

        if (!_eventTypeMap.ContainsKey(eventType))
        {
            return EventValidationResult.Failure($"The event type '{eventType}' is not supported.");
        }

        Type? eventTypeInstance = GetEventType(_eventTypeMap, eventType);
        if (eventTypeInstance != null)
        {
            try
            {
                using MemoryStream stream = new(
                    System.Text.Encoding.UTF8.GetBytes(eventData.RootElement.ToString())
                );

                JsonSerializerOptions options = new()
                {
                    PropertyNameCaseInsensitive = true,
                    ReferenceHandler = ReferenceHandler.Preserve,
                };

                object? data = await JsonSerializer.DeserializeAsync(
                    stream,
                    eventTypeInstance,
                    options,
                    cancellationToken
                );
                if (data == null)
                {
                    return EventValidationResult.Failure(
                        $"The data for event type '{eventType}' is invalid."
                    );
                }

                List<ValidationResult> results = new();
                bool isValid = Validator.TryValidateObject(
                    data,
                    new ValidationContext(data),
                    results,
                    true
                );

                if (!isValid)
                {
                    return EventValidationResult.Failure(
                        results.Select(r => r.ErrorMessage ?? "Unknown error").ToList()
                    );
                }
            }
            catch (JsonException ex)
            {
                return EventValidationResult.Failure(ex.Message);
            }
        }

        return EventValidationResult.Success();
    }

    private static Type? GetEventType(Dictionary<string, Type> eventTypeMap, string eventType)
    {
        eventTypeMap.TryGetValue(eventType, out Type? type);
        return type;
    }
}
