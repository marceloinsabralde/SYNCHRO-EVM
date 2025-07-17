// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kumara.Common.Controllers.Responses;

public class PaginationLinks
{
    public required PaginationLink Self { get; set; }

    public PaginationLink? Next { get; set; }
}

public record PaginationLink(string Href);
