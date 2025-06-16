// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Kumara.Common.Controllers.Responses;
using Kumara.WebApi.Controllers.Responses;
using Kumara.WebApi.Database;
using Microsoft.AspNetCore.Mvc;

namespace Kumara.WebApi.Controllers;

[Route("api/v1/material-activity-allocations")]
[ApiController]
public class MaterialActivityAllocationsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [EndpointName("ListMaterialActivityAllocations")]
    public ActionResult<ListResponse<MaterialActivityAllocationResponse>> Index(
        [Required] Guid iTwinId,
        Guid? activityId,
        Guid? materialId
    )
    {
        var allocations = dbContext.MaterialActivityAllocations.Where(allocation =>
            allocation.ITwinId == iTwinId
        );

        if (activityId is not null)
            allocations = allocations.Where(allocation => allocation.ActivityId == activityId);

        if (materialId is not null)
            allocations = allocations.Where(allocation => allocation.MaterialId == materialId);

        if (!allocations.Any())
            return NotFound();

        return Ok(
            new ListResponse<MaterialActivityAllocationResponse>
            {
                items = allocations.Select(allocation =>
                    MaterialActivityAllocationResponse.FromMaterialActivityAllocation(allocation)
                ),
            }
        );
    }
}
