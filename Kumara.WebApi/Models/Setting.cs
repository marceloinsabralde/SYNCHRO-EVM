// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Nodes;
using Kumara.WebApi.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kumara.WebApi.Models;

[Index(nameof(ITwinId), nameof(Key), IsUnique = true)]
public class Setting<TKey> : ApplicationEntity, IValidatableObject
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

        try
        {
            var _ = Value;
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
    }
}
