// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Kumara.Common.Controllers.Responses;
using Kumara.WebApi.Controllers.Requests;
using Kumara.WebApi.Controllers.Responses;
using Kumara.WebApi.Database;
using Kumara.WebApi.Enums;
using Kumara.WebApi.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Kumara.WebApi.Controllers;

[Route("api/v1/activities")]
[ApiController]
public class ActivitiesController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [EndpointName("ListActivities")]
    public ActionResult<ListResponse<ActivityResponse>> Index(
        [Required] Guid iTwinId,
        Guid? controlAccountId
    )
    {
        ListActivitiesQuery query = new(dbContext, iTwinId);
        ListActivitiesQuery.QueryFilter filter = new() { ControlAccountId = controlAccountId };

        var activities = query.ApplyFilter(filter).ExecuteQuery().Items;

        if (!activities.Any())
            return NotFound();

        return Ok(
            new ListResponse<ActivityResponse>
            {
                Items = activities.Select(act => ActivityResponse.FromActivity(act)),
            }
        );
    }

    [HttpGet("{id}")]
    [EndpointName("GetActivity")]
    public ActionResult<ShowResponse<ActivityResponse>> Show([Required] Guid id)
    {
        var activity = dbContext.Activities.Find(id);

        if (activity is null)
            return NotFound();

        return Ok(
            new ShowResponse<ActivityResponse> { Item = ActivityResponse.FromActivity(activity) }
        );
    }

    [HttpPatch("{id}")]
    [EndpointName("UpdateActivity")]
    public ActionResult<UpdatedResponse<ActivityResponse>> Update(
        [Required] Guid id,
        [FromBody] ActivityUpdateRequest activityUpdate
    )
    {
        var activity = dbContext.Activities.Find(id);

        if (activity is null)
            return NotFound();

        if (activityUpdate.HasChanged(nameof(activityUpdate.ActualStart)))
            activity.ActualStart = activityUpdate.ActualStart;

        if (activityUpdate.HasChanged(nameof(activityUpdate.ActualFinish)))
            activity.ActualFinish = activityUpdate.ActualFinish;

        if (activityUpdate.HasChanged(nameof(activityUpdate.PercentComplete)))
        {
            if (activity.ProgressType is not ActivityProgressType.Manual)
                ModelState.AddModelError(
                    nameof(activityUpdate.PercentComplete),
                    $"cannot be updated on an Activity with the progressType: {activity.ProgressType}"
                );

            activity.PercentComplete = activityUpdate.PercentComplete;
        }

        if (ModelState.IsValid is not true)
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        dbContext.SaveChanges();

        return Accepted(
            new UpdatedResponse<ActivityResponse> { Item = new IdResponse { Id = activity.Id } }
        );
    }
}
