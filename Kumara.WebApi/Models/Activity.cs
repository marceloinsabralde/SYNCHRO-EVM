// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations.Schema;
using Kumara.Common.Database;
using Kumara.WebApi.Enums;
using Kumara.WebApi.Helpers;
using Kumara.WebApi.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NodaTime;

namespace Kumara.WebApi.Models;

[EntityTypeConfiguration(typeof(Activity.Configuration))]
public class Activity : ApplicationEntity, IPageableEntity
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

    public Guid ControlAccountId { get; set; }

    public ControlAccount? ControlAccount { get; set; }

    public required string ReferenceCode { get; set; }
    public required string Name { get; set; }
    public decimal PercentComplete { get; set; } = 0.0m;
    public ActivityProgressType ProgressType { get; set; }

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

    public class Configuration : IEntityTypeConfiguration<Activity>
    {
        public void Configure(EntityTypeBuilder<Activity> builder)
        {
            builder.Property(a => a._actualStart).HasColumnName("actual_start");
            builder.Property(a => a._actualStartHasTime).HasColumnName("actual_start_has_time");
            builder.Property(a => a._actualFinish).HasColumnName("actual_finish");
            builder.Property(a => a._actualFinishHasTime).HasColumnName("actual_finish_has_time");
            builder.Property(a => a._plannedStart).HasColumnName("planned_start");
            builder.Property(a => a._plannedStartHasTime).HasColumnName("planned_start_has_time");
            builder.Property(a => a._plannedFinish).HasColumnName("planned_finish");
            builder.Property(a => a._plannedFinishHasTime).HasColumnName("planned_finish_has_time");
            builder
                .Property(a => a.ProgressType)
                .HasConversion(v => v.ToString(), v => Enum.Parse<ActivityProgressType>(v));
        }
    }
}
