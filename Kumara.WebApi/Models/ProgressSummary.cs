// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.ComponentModel.DataAnnotations.Schema;
using Kumara.Database;
using Microsoft.EntityFrameworkCore;

namespace Kumara.Models;

[PrimaryKey(
    nameof(ITwinId),
    nameof(ActivityId),
    nameof(MaterialId),
    nameof(QuantityUnitOfMeasureId)
)]
[SqlView(Name = "progress_summaries", SqlFileName = "ProgressSummary.sql")]
public class ProgressSummary
{
    [Column("itwin_id")]
    public Guid ITwinId { get; set; }

    public Guid ActivityId { get; set; }
    public Guid MaterialId { get; set; }
    public Guid QuantityUnitOfMeasureId { get; set; }
    public decimal QuantityAtComplete { get; set; }
    public decimal QuantityToDate { get; set; }
    public decimal QuantityToComplete { get; set; }

    public List<RecentProgressEntry>? RecentProgressEntries { get; set; }
}

public class RecentProgressEntry
{
    public Guid Id { get; set; }
    public decimal QuantityDelta { get; set; }
    public DateOnly ProgressDate { get; set; }
}
