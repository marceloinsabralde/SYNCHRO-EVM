// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json.Serialization;

namespace Kumara.EventSource.Models;

/// <summary>
/// Contains links for navigating between pages of results.
/// </summary>
public class PaginationLinks
{
    /// <summary>
    /// Link to the current page.
    /// </summary>
    public PaginationLink Self { get; set; } = new();

    /// <summary>
    /// Link to the next page, if available.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PaginationLink? Next { get; set; }
}
