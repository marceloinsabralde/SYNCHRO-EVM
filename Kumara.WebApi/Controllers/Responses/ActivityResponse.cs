// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using Kumara.Models;
using Kumara.Types;

namespace Kumara.WebApi.Controllers.Responses;

public class ActivityResponse
{
    public Guid Id { get; set; }

    public Guid ITwinId { get; set; }

    public Guid ControlAccountId { get; set; }

    public required string ReferenceCode { get; set; }
    public required string Name { get; set; }
    public DateWithOptionalTime? ActualStart { get; set; }
    public DateWithOptionalTime? ActualFinish { get; set; }
    public DateTimeOffset? PlannedStart { get; set; }
    public DateTimeOffset? PlannedFinish { get; set; }

    public static ActivityResponse FromActivity(Activity activity)
    {
        var response = new ActivityResponse
        {
            Id = activity.Id,
            ITwinId = activity.ITwinId,
            ControlAccountId = activity.ControlAccountId,
            ReferenceCode = activity.ReferenceCode,
            Name = activity.Name,
            PlannedStart = activity.PlannedStart,
            PlannedFinish = activity.PlannedFinish,
        };

        if (activity.ActualStart is not null)
        {
            response.ActualStart = new DateWithOptionalTime
            {
                DateTime = activity.ActualStart.Value,
                HasTime = activity.ActualStartHasTime.GetValueOrDefault(),
            };
        }

        if (activity.ActualFinish is not null)
        {
            response.ActualFinish = new DateWithOptionalTime
            {
                DateTime = activity.ActualFinish.Value,
                HasTime = activity.ActualFinishHasTime.GetValueOrDefault(),
            };
        }

        return response;
    }
}
