// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json.Serialization;

namespace Kumara.EventSource.Models;

/// <summary>
/// Represents a paginated list of items with navigation links.
/// </summary>
/// <typeparam name="T">The type of items in the list</typeparam>
public class PaginatedList<T>
{
    /// <summary>
    /// The collection of items in the current page.
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Navigation links for pagination.
    /// </summary>
    public PaginationLinks Links { get; set; } = new();

    /// <summary>
    /// Indicates whether there are more items available beyond the current page.
    /// </summary>
    [JsonIgnore]
    public bool HasMoreItems { get; set; }
}

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

/// <summary>
/// Represents a hypermedia link.
/// </summary>
public class PaginationLink
{
    /// <summary>
    /// The URL of the link.
    /// </summary>
    public string Href { get; set; } = string.Empty;
}
