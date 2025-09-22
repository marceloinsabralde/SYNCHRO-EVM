// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations.Schema;
using Kumara.Common.Database;
using Kumara.WebApi.Helpers;
using Kumara.WebApi.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NodaTime;

namespace Kumara.WebApi.Models;

[EntityTypeConfiguration(typeof(ControlAccount.Configuration))]
public class ControlAccount : ApplicationEntity, IPageableEntity
{
    private Instant? _actualStart;
    private bool? _actualStartHasTime;
    private Instant? _actualFinish;
    private bool? _actualFinishHasTime;
    private Instant? _plannedStart;
    private bool? _plannedStartHasTime;
    private Instant? _plannedFinish;
    private bool? _plannedFinishHasTime;

    public Guid Id { get; set; }

    public Guid ITwinId { get; set; }
    public Guid TaskId { get; set; }

    public required string ReferenceCode { get; set; }
    public required string Name { get; set; }
    public decimal PercentComplete { get; set; } = 0.0m;

    [NotMapped]
    public DateWithOptionalTime? ActualStart
    {
        get =>
            DateWithOptionalTimeHelper.GetFromBackingFields(
                ref _actualStart,
                ref _actualStartHasTime
            );
        set =>
            DateWithOptionalTimeHelper.SetBackingFields(
                value,
                ref _actualStart,
                ref _actualStartHasTime
            );
    }

    [NotMapped]
    public DateWithOptionalTime? ActualFinish
    {
        get =>
            DateWithOptionalTimeHelper.GetFromBackingFields(
                ref _actualFinish,
                ref _actualFinishHasTime
            );
        set =>
            DateWithOptionalTimeHelper.SetBackingFields(
                value,
                ref _actualFinish,
                ref _actualFinishHasTime
            );
    }

    [NotMapped]
    public DateWithOptionalTime? PlannedStart
    {
        get =>
            DateWithOptionalTimeHelper.GetFromBackingFields(
                ref _plannedStart,
                ref _plannedStartHasTime
            );
        set =>
            DateWithOptionalTimeHelper.SetBackingFields(
                value,
                ref _plannedStart,
                ref _plannedStartHasTime
            );
    }

    [NotMapped]
    public DateWithOptionalTime? PlannedFinish
    {
        get =>
            DateWithOptionalTimeHelper.GetFromBackingFields(
                ref _plannedFinish,
                ref _plannedFinishHasTime
            );
        set =>
            DateWithOptionalTimeHelper.SetBackingFields(
                value,
                ref _plannedFinish,
                ref _plannedFinishHasTime
            );
    }

    public class Configuration : IEntityTypeConfiguration<ControlAccount>
    {
        public void Configure(EntityTypeBuilder<ControlAccount> builder)
        {
            builder.Property(ca => ca._actualStart).HasColumnName("actual_start");
            builder.Property(ca => ca._actualStartHasTime).HasColumnName("actual_start_has_time");
            builder.Property(ca => ca._actualFinish).HasColumnName("actual_finish");
            builder.Property(ca => ca._actualFinishHasTime).HasColumnName("actual_finish_has_time");
            builder.Property(ca => ca._plannedStart).HasColumnName("planned_start");
            builder.Property(ca => ca._plannedStartHasTime).HasColumnName("planned_start_has_time");
            builder.Property(ca => ca._plannedFinish).HasColumnName("planned_finish");
            builder
                .Property(ca => ca._plannedFinishHasTime)
                .HasColumnName("planned_finish_has_time");
        }
    }
}
