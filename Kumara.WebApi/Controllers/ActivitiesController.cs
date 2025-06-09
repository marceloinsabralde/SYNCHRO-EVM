// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.ComponentModel.DataAnnotations;
using Kumara.Database;
using Kumara.WebApi.Controllers.Requests;
using Kumara.WebApi.Controllers.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Kumara.WebApi.Controllers;

[Route("api/v1/activities")]
[ApiController]
public class ActivitiesController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public IActionResult Index([Required] Guid iTwinId, Guid? controlAccountId)
    {
        var activities = dbContext.Activities.Where(act => act.ITwinId == iTwinId);

        if (controlAccountId is not null)
            activities = activities.Where(act => act.ControlAccountId == controlAccountId);

        if (!activities.Any())
            return NotFound();

        return Ok(
            new ListResponse<ActivityResponse>
            {
                items = activities.Select(act => ActivityResponse.FromActivity(act)),
            }
        );
    }

    [HttpGet("{id}")]
    public IActionResult Show([Required] Guid id)
    {
        var activity = dbContext.Activities.Find(id);

        if (activity is null)
            return NotFound();

        return Ok(ActivityResponse.FromActivity(activity));
    }

    [HttpPatch("{id}")]
    public IActionResult Update([Required] Guid id, [FromBody] ActivityUpdateRequest activityUpdate)
    {
        var activity = dbContext.Activities.Find(id);

        if (activity is null)
            return NotFound();

        if (activityUpdate.HasChanged(nameof(activityUpdate.ActualStart)))
            activity.ActualStart = activityUpdate.ActualStart;

        if (activityUpdate.HasChanged(nameof(activityUpdate.ActualFinish)))
            activity.ActualFinish = activityUpdate.ActualFinish;

        dbContext.SaveChanges();

        return Accepted();
    }
}
