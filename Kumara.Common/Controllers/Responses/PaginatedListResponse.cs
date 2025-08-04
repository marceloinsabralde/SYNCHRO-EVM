// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json.Serialization;

namespace Kumara.Common.Controllers.Responses;

public class PaginatedListResponse<T> : ListResponse<T>
    where T : notnull
{
    [JsonPropertyName("_links")]
    public required PaginationLinks Links { get; set; }
}
