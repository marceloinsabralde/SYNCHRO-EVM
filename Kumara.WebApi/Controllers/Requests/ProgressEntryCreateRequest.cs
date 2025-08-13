// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Kumara.Common.Validations;
using NodaTime;

namespace Kumara.WebApi.Controllers.Requests;

public class ProgressEntryCreateRequest
{
    [NotEmpty]
    public Guid? id { get; set; }

    [NotEmpty]
    public Guid iTwinId { get; set; }

    [NotEmpty]
    public Guid activityId { get; set; }

    [NotEmpty]
    public Guid materialId { get; set; }

    [NotEmpty]
    public Guid quantityUnitOfMeasureId { get; set; }

    [Required]
    public decimal quantityDelta { get; set; }

    [Required]
    public LocalDate progressDate { get; set; }
}
