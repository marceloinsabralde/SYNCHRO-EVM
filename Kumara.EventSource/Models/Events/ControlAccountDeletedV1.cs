// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Kumara.EventSource.Models.Events;

public class ControlAccountDeletedV1
{
    [Required]
    public required Guid Id { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string EventTypeVersion => "1.0";
}
