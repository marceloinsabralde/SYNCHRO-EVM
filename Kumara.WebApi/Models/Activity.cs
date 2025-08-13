// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations.Schema;
using Kumara.Common.Database;
using Kumara.WebApi.Enums;
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
        get
        {
            if (_actualStart is null)
                return null;

            return new DateWithOptionalTime
            {
                DateTime = _actualStart.Value.WithOffset(Offset.Zero),
                HasTime = _actualStartHasTime.GetValueOrDefault(),
            };
        }
        set
        {
            _actualStart = value?.DateTime.ToInstant();
            _actualStartHasTime = value?.HasTime;
        }
    }

    [NotMapped]
    public DateWithOptionalTime? ActualFinish
    {
        get
        {
            if (_actualFinish is null)
                return null;

            return new DateWithOptionalTime
            {
                DateTime = _actualFinish.Value.WithOffset(Offset.Zero),
                HasTime = _actualFinishHasTime.GetValueOrDefault(),
            };
        }
        set
        {
            _actualFinish = value?.DateTime.ToInstant();
            _actualFinishHasTime = value?.HasTime;
        }
    }

    public OffsetDateTime? PlannedStart { get; set; }
    public OffsetDateTime? PlannedFinish { get; set; }

    public class Configuration : IEntityTypeConfiguration<Activity>
    {
        public void Configure(EntityTypeBuilder<Activity> builder)
        {
            builder.Property(a => a._actualStart).HasColumnName("actual_start");
            builder.Property(a => a._actualStartHasTime).HasColumnName("actual_start_has_time");
            builder.Property(a => a._actualFinish).HasColumnName("actual_finish");
            builder.Property(a => a._actualFinishHasTime).HasColumnName("actual_finish_has_time");
            builder
                .Property(a => a.ProgressType)
                .HasConversion(v => v.ToString(), v => Enum.Parse<ActivityProgressType>(v));
        }
    }
}
