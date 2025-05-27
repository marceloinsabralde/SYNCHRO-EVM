// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Kumara.EventSource.Models.Events;

public class ActivityCreatedV1
{
    [Required]
    public required Guid Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    public required string Name { get; set; }

    [Required(AllowEmptyStrings = false)]
    public required string ReferenceCode { get; set; }

    public Guid ControlAccountId { get; set; }

    public DateTimeOffset? PlannedStart { get; set; }

    public DateTimeOffset? PlannedFinish { get; set; }

    public DateTimeOffset? ActualStart { get; set; }

    public DateTimeOffset? ActualFinish { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string EventTypeVersion => "1.0";
}
