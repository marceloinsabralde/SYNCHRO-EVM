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

[Route("api/v1/units-of-measure")]
[ApiController]
public class UnitsOfMeasureController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [EndpointName("ListUnitsOfMeasure")]
    public ActionResult<ListResponse<UnitOfMeasureResponse>> Index(
        [Required] Guid iTwinId,
        [FromQuery(Name = "$continuationToken")]
            ContinuationToken<ListUnitsOfMeasureQueryFilter>? continuationToken,
        [FromQuery(Name = "$top")] int limit = 50
    )
    {
        ListUnitsOfMeasureQuery query = new ListUnitsOfMeasureQuery(
            dbContext.UnitsOfMeasure.AsQueryable(),
            iTwinId
        );
        ListUnitsOfMeasureQueryFilter filter;

        if (continuationToken is not null)
            filter = continuationToken.Value;
        else
            filter = new();

        var result = query.ApplyFilter(filter).WithLimit(limit).ExecuteQuery();
        var unitsOfMeasure = result.Items;

        if (!unitsOfMeasure.Any())
        {
            return NotFound();
        }

        return Ok(
            this.BuildPaginatedResponse(
                unitsOfMeasure.Select(UnitOfMeasureResponse.FromUnitOfMeasure),
                result,
                filter
            )
        );
    }
}
