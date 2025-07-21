// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations.Schema;
using Kumara.Common.Database;

namespace Kumara.WebApi.Models;

public class ControlAccount : ApplicationEntity, IPageableEntity
{
    public Guid Id { get; set; }

    public Guid ITwinId { get; set; }
    public Guid TaskId { get; set; }

    public required string ReferenceCode { get; set; }
    public required string Name { get; set; }
    public decimal PercentComplete { get; set; } = 0.0m;
    public DateOnly? ActualStart { get; set; }
    public DateOnly? ActualFinish { get; set; }
    public DateOnly? PlannedStart { get; set; }
    public DateOnly? PlannedFinish { get; set; }
}
