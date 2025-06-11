// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

namespace Kumara.Common.Controllers.Responses;

public class UpdatedResponse<T>
{
    public required IdResponse item { get; set; }
}
