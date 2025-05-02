using System.ComponentModel.DataAnnotations;
using Kumara.Database;
using Kumara.WebApi.Controllers.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Kumara.WebApi.Controllers;

[Route("api/v1/activities")]
[ApiController]
public class ActivitiesController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public IActionResult Index([Required] Guid iTwinId)
    {
        var activities = dbContext.Activities.Where(act => act.ITwinId == iTwinId);

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
}
