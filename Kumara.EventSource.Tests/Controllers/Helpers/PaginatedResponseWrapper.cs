// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.Models;

namespace Kumara.EventSource.Tests.Controllers.Helpers;

public class PaginatedResponseWrapper
{
    public List<Event> Items { get; set; } = new();
    public PaginationLinksResponse Links { get; set; } = new();

    public List<Event> GetEvents()
    {
        return Items;
    }
}

public class PaginationLinksResponse
{
    public PaginationLinkResponse Self { get; set; } = new();
    public PaginationLinkResponse? Next { get; set; }
}

public class PaginationLinkResponse
{
    public string Href { get; set; } = string.Empty;
}
