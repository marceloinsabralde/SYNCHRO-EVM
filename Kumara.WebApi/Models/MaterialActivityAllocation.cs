// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kumara.Models;

public class MaterialActivityAllocation
{
    public Guid Id { get; set; }

    [Column("itwin_id")]
    public Guid ITwinId { get; set; }

    [Required]
    public Guid MaterialId { get; set; }
    public required Material Material { get; set; }

    [Required]
    public Guid ActivityId { get; set; }
    public required Activity Activity { get; set; }

    [Required]
    public Guid QuantityUnitOfMeasureId { get; set; }
    public required UnitOfMeasure QuantityUnitOfMeasure { get; set; }

    public required decimal QuantityAtComplete { get; set; }
}
