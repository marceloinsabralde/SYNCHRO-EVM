// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Kumara.Common.Controllers.Responses;
using Kumara.WebApi.Controllers.Responses;
using Kumara.WebApi.Database;
using Microsoft.AspNetCore.Mvc;

namespace Kumara.WebApi.Controllers;

[Route("api/v1/units-of-measure")]
[ApiController]
public class UnitsOfMeasureController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [EndpointName("ListUnitsOfMeasure")]
    public ActionResult<ListResponse<UnitOfMeasureResponse>> Index([Required] Guid iTwinId)
    {
        var unitsOfMeasure = dbContext.UnitsOfMeasure.Where(uom => uom.ITwinId == iTwinId);

        if (!unitsOfMeasure.Any())
        {
            return NotFound();
        }

        return Ok(
            new ListResponse<UnitOfMeasureResponse>
            {
                Items = unitsOfMeasure.Select(uom => UnitOfMeasureResponse.FromUnitOfMeasure(uom)),
            }
        );
    }
}
