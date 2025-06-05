// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kumara.Models;

public class Material : ApplicationEntity
{
    public Guid Id { get; set; }

    [Column("itwin_id")]
    public Guid ITwinId { get; set; }

    public required string Name { get; set; }

    public Guid ResourceRoleId { get; set; }

    public Guid QuantityUnitOfMeasureId { get; set; }
    public required UnitOfMeasure QuantityUnitOfMeasure { get; set; }
}
