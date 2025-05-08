using Kumara.Models;

namespace Kumara.WebApi.Controllers.Responses;

public class MaterialResponse
{
    public Guid Id { get; set; }

    public Guid ITwinId { get; set; }
    public required Guid ResourceRoleId { get; set; }
    public Guid QuantityUnitOfMeasureId { get; set; }
    public required string Name { get; set; }

    public static MaterialResponse FromMaterial(Material material)
    {
        return new MaterialResponse
        {
            Id = material.Id,
            ITwinId = material.ITwinId,
            ResourceRoleId = material.ResourceRoleId,
            QuantityUnitOfMeasureId = material.QuantityUnitOfMeasureId,
            Name = material.Name,
        };
    }
}
