// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json.Serialization;

namespace Kumara.EventSource.Models;

public class PaginatedList<T>
{
    public List<T> Items { get; set; } = new();
    public PaginationLinks Links { get; set; } = new();

    [JsonIgnore]
    public bool HasMoreItems { get; set; }
}

public class PaginationLinks
{
    public PaginationLink Self { get; set; } = new();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PaginationLink? Next { get; set; }
}

public class PaginationLink
{
    public string Href { get; set; } = string.Empty;
}
