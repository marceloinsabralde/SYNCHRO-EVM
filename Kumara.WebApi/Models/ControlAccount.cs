// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Database;
using NodaTime;

namespace Kumara.WebApi.Models;

public class ControlAccount : ApplicationEntity, IPageableEntity
{
    public Guid Id { get; set; }

    public Guid ITwinId { get; set; }
    public Guid TaskId { get; set; }

    public required string ReferenceCode { get; set; }
    public required string Name { get; set; }
    public decimal PercentComplete { get; set; } = 0.0m;
    public LocalDate? ActualStart { get; set; }
    public LocalDate? ActualFinish { get; set; }
    public LocalDate? PlannedStart { get; set; }
    public LocalDate? PlannedFinish { get; set; }
}
