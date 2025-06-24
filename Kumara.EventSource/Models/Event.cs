// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kumara.Common.Utilities;

namespace Kumara.EventSource.Models;

public class Event
{
    [Required]
    public Guid Id { get; set; } = GuidUtility.CreateGuid();

    [Required]
    public required Guid ITwinId { get; set; }

    [Required]
    public required Guid AccountId { get; set; }

    [Required]
    public required string CorrelationId { get; set; }

    [Required]
    public Guid IdempotencyId { get; set; } = GuidUtility.CreateGuid();

    [Required]
    public required string SpecVersion { get; set; }

    [Required]
    public required Uri Source { get; set; }

    [Required]
    public required string Type { get; set; }

    public DateTimeOffset? Time { get; set; }

    [Required]
    [JsonPropertyName("data")]
    public JsonDocument DataJson { get; set; } = JsonDocument.Parse("{}");
}
