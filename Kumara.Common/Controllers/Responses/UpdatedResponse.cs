// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Kumara.Common.Controllers.Responses;

public class UpdatedResponse<T>
    where T : class
{
    [Required]
    public required IdResponse item { get; set; }
}
