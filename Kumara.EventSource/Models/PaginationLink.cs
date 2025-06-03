// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

namespace Kumara.EventSource.Models;

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
