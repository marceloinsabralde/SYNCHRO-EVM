// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.ComponentModel.DataAnnotations.Schema;

namespace Kumara.Models;

public class Activity
{
    public Guid Id { get; set; }

    [Column("itwin_id")]
    public Guid ITwinId { get; set; }

    public Guid ControlAccountId { get; set; }

    public ControlAccount? ControlAccount { get; set; }

    public required string ReferenceCode { get; set; }
    public required string Name { get; set; }
    public DateTimeOffset? ActualStart { get; set; }
    public bool? ActualStartHasTime { get; set; }
    public DateTimeOffset? ActualFinish { get; set; }
    public bool? ActualFinishHasTime { get; set; }
    public DateTimeOffset? PlannedStart { get; set; }
    public DateTimeOffset? PlannedFinish { get; set; }
}
