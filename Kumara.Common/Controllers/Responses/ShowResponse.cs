// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

namespace Kumara.Common.Controllers.Responses;

public class ShowResponse<T>
{
    public required T item { get; set; }
}
