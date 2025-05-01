// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kumara.Models;

public class ProgressEntry
{
    public Guid Id { get; set; }

    [Column("itwin_id")]
    public required Guid ITwinId { get; set; }

    [Required]
    public Guid? ActivityId { get; set; }
    public required Activity Activity { get; set; }

    [Required]
    public Guid? MaterialId { get; set; }
    public required Material Material { get; set; }

    [Required]
    public Guid? QuantityUnitOfMeasureId { get; set; }
    public required UnitOfMeasure QuantityUnitOfMeasure { get; set; }

    public required decimal QuantityDelta { get; set; }

    public required DateOnly ProgressDate { get; set; }
}
