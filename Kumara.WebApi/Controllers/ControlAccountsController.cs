// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.ComponentModel.DataAnnotations;
using Kumara.Database;
using Kumara.WebApi.Controllers.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Kumara.WebApi.Controllers;

[Route("api/v1/control-accounts")]
[ApiController]
public class ControlAccountsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public IActionResult Index([Required] Guid iTwinId)
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
