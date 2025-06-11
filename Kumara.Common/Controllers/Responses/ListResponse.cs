// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Kumara.Common.Controllers.Responses;

public class ListResponse<T>
    where T : class
{
    [Required]
    public required IEnumerable<T> items { get; set; }
}
