// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kumara.Common.Utilities;
using Kumara.Common.Validations;
using Kumara.EventSourceToo.Validations;
using NodaTime;

namespace Kumara.EventSourceToo.Controllers.Requests;

public class EventCreateRequest : IValidatableObject
{
    [NotEmpty]
    public Guid? Id { get; set; }

    [NotEmpty]
    public Guid ITwinId { get; set; }

    [NotEmpty]
    public Guid AccountId { get; set; }

    [NotEmpty]
    public string? CorrelationId { get; set; }

    [NotEmpty]
    [ValidEventType]
    public required string Type { get; set; }

    [Required]
    public required JsonDocument Data { get; set; }

    public Guid? TriggeredByUserSubject { get; set; }

    public Instant? TriggeredByUserAt { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) =>
        ValidateData();

    private IEnumerable<ValidationResult> ValidateData()
    {
        object? dataObject = null;
        try
        {
            TryDeserializeDataObject(out dataObject);
        }
        catch (JsonException) { } // TODO: Capture exception message without exposing internals.

        if (!TryValidateDataObject(dataObject, out List<ValidationResult> results))
        {
            List<string> errorMessageParts =
            [
                $"The Data field does not conform to the \"{Type}\" Event Type.",
            ];

            results.ForEach(result =>
            {
                if (result.ErrorMessage is not null)
                    errorMessageParts.Add(result.ErrorMessage);
            });

            yield return new ValidationResult(string.Join(" ", errorMessageParts), [nameof(Data)]);
        }
    }

    private bool TryDeserializeDataObject(out object? eventDataObject)
    {
        eventDataObject = null;
        EventTypeRegistry.Instance.TryGetEventType(Type, out Type? eventType);

        if (eventType is null)
            return false;

        eventDataObject = Data.Deserialize(eventType, JsonSerializerOptions);
        return eventDataObject is not null;
    }

    private static bool TryValidateDataObject(
        object? eventDataObject,
        out List<ValidationResult> results
    )
    {
        results = [];

        if (eventDataObject is null)
            return false;

        var context = new ValidationContext(eventDataObject);
        return Validator.TryValidateObject(eventDataObject, context, results, true);
    }

    private static readonly JsonSerializerOptions JsonSerializerOptions = new(
        JsonSerializerDefaults.Web
    )
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };
}
