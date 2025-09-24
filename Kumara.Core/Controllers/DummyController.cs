// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Microsoft.AspNetCore.Mvc;

namespace Kumara.Core.Controllers;

[Route("api/v1/dummy")]
[ApiController]
public class DummyController : ControllerBase
{
    [HttpGet]
    [EndpointName("GetDummy")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    public IActionResult GetDummy()
    {
        return NoContent();
    }
}
