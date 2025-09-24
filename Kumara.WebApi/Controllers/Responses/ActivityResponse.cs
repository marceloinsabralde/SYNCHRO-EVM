// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.WebApi.Enums;
using Kumara.WebApi.Models;
using Kumara.WebApi.Types;
using NodaTime;

namespace Kumara.WebApi.Controllers.Responses;

public class ActivityResponse
{
    public Guid Id { get; set; }

    public Guid ITwinId { get; set; }

    public Guid ControlAccountId { get; set; }

    public required string ReferenceCode { get; set; }
    public required string Name { get; set; }
    public decimal PercentComplete { get; set; }
    public ActivityProgressType ProgressType { get; set; }
    public DateWithOptionalTime? ActualStart { get; set; }
    public DateWithOptionalTime? ActualFinish { get; set; }
    public DateWithOptionalTime? PlannedStart { get; set; }
    public DateWithOptionalTime? PlannedFinish { get; set; }

    public Instant CreatedAt { get; set; }

    public Instant UpdatedAt { get; set; }

    public static ActivityResponse FromActivity(Activity activity)
    {
        return new ActivityResponse
        {
            Id = activity.Id,
            ITwinId = activity.ITwinId,
            ControlAccountId = activity.ControlAccountId,
            ReferenceCode = activity.ReferenceCode,
            Name = activity.Name,
            PercentComplete = activity.PercentComplete,
            ActualStart = activity.ActualStart,
            ActualFinish = activity.ActualFinish,
            PlannedStart = activity.PlannedStart,
            PlannedFinish = activity.PlannedFinish,
            CreatedAt = activity.CreatedAt,
            UpdatedAt = activity.UpdatedAt,
        };
    }
}
