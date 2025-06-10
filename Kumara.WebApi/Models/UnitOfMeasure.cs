// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace Kumara.Models;

public class UnitOfMeasure : ApplicationEntity
{
    public Guid Id { get; set; }

    [Column("itwin_id")]
    public required Guid ITwinId { get; set; }

    public required string Name { get; set; }
    public required string Symbol { get; set; }
}
