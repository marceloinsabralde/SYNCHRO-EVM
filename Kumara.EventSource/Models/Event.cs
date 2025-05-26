// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kumara.EventSource.Utilities;

namespace Kumara.EventSource.Models;

public class Event
{
    [Required]
    [JsonPropertyName("iTwinId")]
    public required Guid ITwinId { get; set; }

    [Required]
    [JsonPropertyName("accountId")]
    public required Guid AccountId { get; set; }

    [Required]
    [JsonPropertyName("correlationId")]
    public required string CorrelationId { get; set; }

    [Required]
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = GuidUtility.CreateGuid();

    [Required]
    [JsonPropertyName("specVersion")]
    public required string SpecVersion { get; set; }

    [Required]
    [JsonPropertyName("source")]
    public required Uri Source { get; set; }

    [Required]
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("time")]
    public DateTimeOffset? Time { get; set; }

    [Required]
    [JsonPropertyName("data")]
    public JsonDocument DataJson { get; set; } = JsonDocument.Parse("{}");
}
