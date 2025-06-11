// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Kumara.WebApi.Controllers.Responses;
using Kumara.WebApi.Database;
using Microsoft.AspNetCore.Mvc;

namespace Kumara.WebApi.Controllers;

[Route("api/v1/materials")]
[ApiController]
public class MaterialsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public IActionResult Index([Required] Guid iTwinId)
    {
        var materials = dbContext.Materials.Where(materials => materials.ITwinId == iTwinId);
        if (!materials.Any())
            return NotFound();

        return Ok(
            new ListResponse<MaterialResponse>
            {
                items = materials.Select(material => MaterialResponse.FromMaterial(material)),
            }
        );
    }

    [HttpGet("{id}")]
    public IActionResult Show([Required] Guid id)
    {
        var material = dbContext.Materials.Find(id);

        if (material is null)
            return NotFound();

        return Ok(MaterialResponse.FromMaterial(material));
    }
}
