// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Kumara.WebApi.Models;

[Index(nameof(ITwinId), nameof(Key), IsUnique = true)]
public class Setting<TRecord, TKey> : ApplicationEntity, IValidatableObject
    where TRecord : class
    where TKey : Enum
{
    private string _value = "null";

    public Guid Id { get; set; }
    public required Guid ITwinId { get; set; }

    public required TKey Key { get; set; }

    [Column(TypeName = "jsonb")]
    public required object Value
    {
        get
        {
            JsonElement element = JsonDocument.Parse(_value).RootElement;

            return element.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Number => element.GetDecimal(),
                JsonValueKind.String => element.GetString()!,
                _ => throw new InvalidOperationException(
                    $"{element.ValueKind} types are not supported"
                ),
            };
        }
        set { _value = JsonSerializer.Serialize(value); }
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        ValidationResult? result = null;

        object? theValue = null;
        try
        {
            theValue = Value;
        }
        catch (InvalidOperationException ex)
        {
            result = new ValidationResult(ex.Message, new[] { nameof(Value) });
        }
        if (result is not null)
        {
            yield return result;
            result = null;
        }

        var property = typeof(TRecord).GetProperty(Key.ToString());
        if (property is null)
        {
            yield return new ValidationResult(
                $"Could not find {Key} in {typeof(TRecord)}",
                new[] { nameof(Key) }
            );
        }
        else
        {
            var record = Activator.CreateInstance<TRecord>();
            try
            {
                property.SetValue(record, theValue);
            }
            catch (ArgumentException)
            {
                result = new ValidationResult(
                    $"{Key} values must be convertable to {property.PropertyType}",
                    new[] { nameof(Value) }
                );
            }
        }
        if (result is not null)
        {
            yield return result;
            result = null;
        }
    }
}
