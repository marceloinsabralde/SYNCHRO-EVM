// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Kumara.EventSource.Models.Events;

public class ActivityUpdatedV1
{
    [Required]
    [JsonPropertyName("id")]
    public required Guid Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("control_account_id")]
    public Guid ControlAccountId { get; set; }

    [JsonPropertyName("planned_start")]
    public DateTimeOffset? PlannedStart { get; set; }

    [JsonPropertyName("planned_finish")]
    public DateTimeOffset? PlannedFinish { get; set; }

    [JsonPropertyName("actual_start")]
    public DateTimeOffset? ActualStart { get; set; }

    [JsonPropertyName("actual_finish")]
    public DateTimeOffset? ActualFinish { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonPropertyName("event_type_version")]
    public string EventTypeVersion => "1.0";
}
