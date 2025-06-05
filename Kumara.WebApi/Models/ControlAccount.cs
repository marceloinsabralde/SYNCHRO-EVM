// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.ComponentModel.DataAnnotations.Schema;

namespace Kumara.Models;

public class ControlAccount : ApplicationEntity
{
    public Guid Id { get; set; }

    [Column("itwin_id")]
    public Guid ITwinId { get; set; }
    public Guid TaskId { get; set; }

    public required string ReferenceCode { get; set; }
    public required string Name { get; set; }
    public DateOnly? ActualStart { get; set; }
    public DateOnly? ActualFinish { get; set; }
    public DateOnly? PlannedStart { get; set; }
    public DateOnly? PlannedFinish { get; set; }
}
