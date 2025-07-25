// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Kumara.Common.Controllers.Extensions;
using Kumara.Common.Controllers.Responses;
using Kumara.Common.Utilities;
using Kumara.WebApi.Controllers.Responses;
using Kumara.WebApi.Database;
using Kumara.WebApi.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Kumara.WebApi.Controllers;

[Route("api/v1/control-accounts")]
[ApiController]
[Produces("application/json")]
public class ControlAccountsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [EndpointName("ListControlAccounts")]
    public ActionResult<PaginatedListResponse<ControlAccountResponse>> Index(
        [Required] Guid iTwinId,
        [FromQuery(Name = "$continuationToken")]
            ContinuationToken<ListControlAccountsQueryFilter>? continuationToken,
        [FromQuery(Name = "$top")] int limit = 50
    )
    {
        ListControlAccountsQuery query = new(dbContext.ControlAccounts.AsQueryable(), iTwinId);
        ListControlAccountsQueryFilter filter;

        if (continuationToken is not null)
            filter = continuationToken.Value;
        else
            filter = new();

        var result = query.ApplyFilter(filter).WithLimit(limit).ExecuteQuery();
        var controlAccounts = result.Items;

        if (!controlAccounts.Any())
        {
            return NotFound();
        }

        return Ok(
            this.BuildPaginatedResponse(
                controlAccounts.Select(ControlAccountResponse.FromControlAccount),
                result,
                filter
            )
        );
    }

    [HttpGet("{id}")]
    [EndpointName("GetControlAccount")]
    public ActionResult<ShowResponse<ControlAccountResponse>> Show([Required] Guid id)
    {
        var controlAccount = dbContext.ControlAccounts.Find(id);

        if (controlAccount is null)
        {
            return NotFound();
        }

        return Ok(
            new ShowResponse<ControlAccountResponse>
            {
                Item = ControlAccountResponse.FromControlAccount(controlAccount),
            }
        );
    }
}
