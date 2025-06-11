// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Kumara.WebApi.Controllers.Responses;
using Kumara.WebApi.Database;
using Kumara.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Kumara.WebApi.Controllers;

[Route("api/v1/progress-summaries")]
[ApiController]
public class ProgressSummariesController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [EndpointName("ListProgressSummaries")]
    public ActionResult<ListResponse<ProgressSummary>> Index(
        [Required] Guid iTwinId,
        Guid? activityId,
        Guid? materialId
    )
    {
        var progressSummaries = dbContext.ProgressSummaries.Where(ps => ps.ITwinId == iTwinId);

        if (activityId is not null)
        {
            progressSummaries = progressSummaries.Where(ps => ps.ActivityId == activityId);
        }

        if (materialId is not null)
        {
            progressSummaries = progressSummaries.Where(ps => ps.MaterialId == materialId);
        }

        if (!progressSummaries.Any())
            return NotFound();

        return Ok(new ListResponse<ProgressSummary> { items = progressSummaries });
    }
}
