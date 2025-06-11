// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Kumara.WebApi.Controllers.Responses;
using Kumara.WebApi.Database;
using Microsoft.AspNetCore.Mvc;

namespace Kumara.WebApi.Controllers;

[Route("api/v1/control-accounts")]
[ApiController]
public class ControlAccountsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [EndpointName("ListControlAccounts")]
    public ActionResult<ListResponse<ControlAccountResponse>> Index([Required] Guid iTwinId)
    {
        var controlAccounts = dbContext.ControlAccounts.Where(ca => ca.ITwinId == iTwinId);

        if (!controlAccounts.Any())
        {
            return NotFound();
        }

        return Ok(
            new ListResponse<ControlAccountResponse>
            {
                items = controlAccounts.Select(ca => ControlAccountResponse.FromControlAccount(ca)),
            }
        );
    }

    [HttpGet("{id}")]
    [EndpointName("GetControlAccount")]
    public IActionResult Show([Required] Guid id)
    {
        var controlAccount = dbContext.ControlAccounts.Find(id);

        if (controlAccount is null)
        {
            return NotFound();
        }

        return Ok(ControlAccountResponse.FromControlAccount(controlAccount));
    }
}
