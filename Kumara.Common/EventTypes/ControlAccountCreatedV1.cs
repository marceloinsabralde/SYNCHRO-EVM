// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Kumara.Common.Attributes;

namespace Kumara.Common.EventTypes;

[EventType("controlaccount.created.v1")]
public class ControlAccountCreatedV1
{
    [Required(AllowEmptyStrings = false)]
    public required string Name { get; set; }

    public string? WbsPath { get; set; }

    public Guid? TaskId { get; set; }

    public DateTimeOffset? PlannedStart { get; set; }

    public DateTimeOffset? PlannedFinish { get; set; }

    public DateTimeOffset? ActualStart { get; set; }

    public DateTimeOffset? ActualFinish { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string EventTypeVersion => "1.0";
}
