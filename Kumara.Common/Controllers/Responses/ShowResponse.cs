// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Kumara.Common.Controllers.Responses;

public class ShowResponse<T> : NamedResponse<T>
    where T : notnull
{
    [Required]
    public required T Item { get; set; }
}
