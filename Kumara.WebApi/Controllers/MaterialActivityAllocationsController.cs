using System.ComponentModel.DataAnnotations;
using Kumara.Database;
using Kumara.WebApi.Controllers.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Kumara.WebApi.Controllers;

[Route("api/v1/material-activity-allocations")]
[ApiController]
public class MaterialActivityAllocationsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public IActionResult Index([Required] Guid iTwinId)
    {
        var allocations = dbContext.MaterialActivityAllocations.Where(act =>
            act.ITwinId == iTwinId
        );

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
