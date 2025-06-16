// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

namespace Kumara.Common.Controllers.Responses;

public class ListResponse<T>
{
    public required IEnumerable<T> items { get; set; }
}
