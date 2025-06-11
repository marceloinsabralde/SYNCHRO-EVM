// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

namespace Kumara.Common.Controllers.Responses;

public class CreatedResponse<T>
{
    public required IdResponse item { get; set; }
}
