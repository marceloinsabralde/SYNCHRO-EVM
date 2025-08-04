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

[Route("api/v1/materials")]
[ApiController]
[Produces("application/json")]
public class MaterialsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [EndpointName("ListMaterials")]
    public ActionResult<PaginatedListResponse<MaterialResponse>> Index(
        [Required] Guid iTwinId,
        [FromQuery(Name = "$continuationToken")]
            ContinuationToken<ListMaterialsQueryFilter>? continuationToken,
        [FromQuery(Name = "$top")] int limit = 50
    )
    {
        ListMaterialsQuery query = new(dbContext.Materials.AsQueryable(), iTwinId);
        ListMaterialsQueryFilter filter;

        if (continuationToken is not null)
            filter = continuationToken.Value;
        else
            filter = new ListMaterialsQueryFilter();

        var result = query.ApplyFilter(filter).WithLimit(limit).ExecuteQuery();
        var materials = result.Items;

        if (!materials.Any())
            return NotFound();

        return Ok(
            this.BuildPaginatedResponse(
                materials.Select(MaterialResponse.FromMaterial),
                result,
                filter
            )
        );
    }

    [HttpGet("{id}")]
    [EndpointName("GetMaterial")]
    public ActionResult<ShowResponse<MaterialResponse>> Show([Required] Guid id)
    {
        var material = dbContext.Materials.Find(id);

        if (material is null)
            return NotFound();

        return Ok(
            new ShowResponse<MaterialResponse> { Item = MaterialResponse.FromMaterial(material) }
        );
    }
}
