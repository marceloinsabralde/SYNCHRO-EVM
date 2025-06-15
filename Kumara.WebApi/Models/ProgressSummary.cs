// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations.Schema;
using Kumara.Common.Database;
using Kumara.WebApi.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NodaTime;

namespace Kumara.WebApi.Models;

[PrimaryKey(
    nameof(ITwinId),
    nameof(ActivityId),
    nameof(MaterialId),
    nameof(QuantityUnitOfMeasureId)
)]
[SqlView(Name = "progress_summaries", SqlFileName = "ProgressSummary.sql")]
[EntityTypeConfiguration(typeof(ProgressSummary.Configuration))]
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

    public class Configuration : IEntityTypeConfiguration<ProgressSummary>
    {
        public void Configure(EntityTypeBuilder<ProgressSummary> builder)
        {
            builder
                .ToView("progress_summaries")
                .OwnsMany(
                    ps => ps.RecentProgressEntries,
                    builder =>
                    {
                        builder.ToJson();
                        builder.Property(pe => pe.Id).HasJsonPropertyName("id");
                        builder
                            .Property(pe => pe.QuantityDelta)
                            .HasJsonPropertyName("quantity_delta");
                        builder
                            .Property(pe => pe.ProgressDate)
                            .HasJsonPropertyName("progress_date");
                        builder
                            .Property(pe => pe.CreatedAt)
                            .HasConversion<InstantJsonConverter>()
                            .HasJsonPropertyName("created_at");
                        builder
                            .Property(pe => pe.UpdatedAt)
                            .HasConversion<InstantJsonConverter>()
                            .HasJsonPropertyName("updated_at");
                    }
                );
        }
    }
}

public class RecentProgressEntry
{
    public Guid Id { get; set; }
    public decimal QuantityDelta { get; set; }
    public DateOnly ProgressDate { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
}
