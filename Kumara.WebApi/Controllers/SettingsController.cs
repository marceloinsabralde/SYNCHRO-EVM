// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel;
using Kumara.Common.Controllers.Responses;
using Kumara.WebApi.Database;
using Kumara.WebApi.Repositories;
using Kumara.WebApi.Types;
using Microsoft.AspNetCore.Mvc;

namespace Kumara.WebApi.Controllers;

[ApiController]
[Route("api/v1/settings")]
public class SettingsController(
    ApplicationDbContext dbContext,
    SettingsRepository settingsRepository
) : ControllerBase
{
    [EndpointName("GetSettings")]
    [EndpointSummary("Get the settings for the specified iTwin.")]
    [HttpGet("{iTwinId}")]
    [Produces("application/json")]
    public async Task<ActionResult<ShowResponse<Settings>>> Get(
        [Description("The iTwin project ID.")] Guid iTwinId
    )
    {
        if (await dbContext.FakeITwins.FindAsync(iTwinId) is null)
        {
            return NotFound();
        }

        var settings = await settingsRepository.FindAsync(iTwinId);
        return Ok(new ShowResponse<Settings> { Item = settings });
    }
}
