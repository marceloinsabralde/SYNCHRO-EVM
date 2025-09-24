// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using NodaTime;

namespace Kumara.WebApi.Models;

public class ProgressEntry : ApplicationEntity
{
    public Guid Id { get; set; }

    public Guid ITwinId { get; set; }

    public Guid ActivityId { get; set; }
    public required Activity Activity { get; set; }

    public Guid MaterialId { get; set; }
    public required Material Material { get; set; }

    public Guid QuantityUnitOfMeasureId { get; set; }
    public required UnitOfMeasure QuantityUnitOfMeasure { get; set; }

    public required decimal QuantityDelta { get; set; }

    public required LocalDate ProgressDate { get; set; }
}
