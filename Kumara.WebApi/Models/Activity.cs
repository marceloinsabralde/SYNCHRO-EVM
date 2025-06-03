// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.ComponentModel.DataAnnotations.Schema;
using Kumara.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kumara.Models;

[EntityTypeConfiguration(typeof(Activity.Configuration))]
public class Activity
{
    private DateTimeOffset? _actualStart;
    private bool? _actualStartHasTime;
    private DateTimeOffset? _actualFinish;
    private bool? _actualFinishHasTime;

    public Guid Id { get; set; }

    [Column("itwin_id")]
    public Guid ITwinId { get; set; }

    public Guid ControlAccountId { get; set; }

    public ControlAccount? ControlAccount { get; set; }

    public required string ReferenceCode { get; set; }
    public required string Name { get; set; }

    [NotMapped]
    public DateWithOptionalTime? ActualStart
    {
        get
        {
            if (_actualStart is null)
                return null;

            return new DateWithOptionalTime
            {
                DateTime = _actualStart.Value,
                HasTime = _actualStartHasTime.GetValueOrDefault(),
            };
        }
        set
        {
            _actualStart = value?.DateTime.ToUniversalTime();
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
                DateTime = _actualFinish.Value,
                HasTime = _actualFinishHasTime.GetValueOrDefault(),
            };
        }
        set
        {
            _actualFinish = value?.DateTime.ToUniversalTime();
            _actualFinishHasTime = value?.HasTime;
        }
    }

    public DateTimeOffset? PlannedStart { get; set; }
    public DateTimeOffset? PlannedFinish { get; set; }

    public class Configuration : IEntityTypeConfiguration<Activity>
    {
        public void Configure(EntityTypeBuilder<Activity> builder)
        {
            builder.Property(a => a._actualStart).HasColumnName("actual_start");
            builder.Property(a => a._actualStartHasTime).HasColumnName("actual_start_has_time");
            builder.Property(a => a._actualFinish).HasColumnName("actual_finish");
            builder.Property(a => a._actualFinishHasTime).HasColumnName("actual_finish_has_time");
        }
    }
}
