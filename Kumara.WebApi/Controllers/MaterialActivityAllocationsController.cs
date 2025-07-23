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

[Route("api/v1/material-activity-allocations")]
[ApiController]
public class MaterialActivityAllocationsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [EndpointName("ListMaterialActivityAllocations")]
    public ActionResult<ListResponse<MaterialActivityAllocationResponse>> Index(
        [Required] Guid iTwinId,
        Guid? activityId,
        Guid? materialId,
        [FromQuery(Name = "$continuationToken")]
            ContinuationToken<ListMaterialActivityAllocationsQueryFilter>? continuationToken,
        [FromQuery(Name = "$top")] int limit = 50
    )
    {
        ListMaterialActivityAllocationsQuery query = new(
            dbContext.MaterialActivityAllocations.AsQueryable(),
            iTwinId
        );
        ListMaterialActivityAllocationsQueryFilter filter;

        if (continuationToken is not null)
            filter = continuationToken.Value;
        else
            filter = new() { ActivityId = activityId, MaterialId = materialId };

        var result = query.ApplyFilter(filter).WithLimit(limit).ExecuteQuery();
        var allocations = result.Items;

        if (!allocations.Any())
            return NotFound();

        return Ok(
            this.BuildPaginatedResponse(
                allocations.Select(
                    MaterialActivityAllocationResponse.FromMaterialActivityAllocation
                ),
                result,
                filter
            )
        );
    }
}
