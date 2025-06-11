// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

namespace Kumara.WebApi.Controllers.Responses;

public class ListResponse<T>
{
    public required IEnumerable<T> items { get; set; }
}
