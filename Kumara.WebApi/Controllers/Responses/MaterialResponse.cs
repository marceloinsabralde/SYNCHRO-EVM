// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using Kumara.Models;
using NodaTime;

namespace Kumara.WebApi.Controllers.Responses;

public class MaterialResponse
{
    public Guid Id { get; set; }

    public Guid ITwinId { get; set; }
    public Guid ResourceRoleId { get; set; }
    public Guid QuantityUnitOfMeasureId { get; set; }
    public required string Name { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }

    public static MaterialResponse FromMaterial(Material material)
    {
        return new MaterialResponse
        {
            Id = material.Id,
            ITwinId = material.ITwinId,
            ResourceRoleId = material.ResourceRoleId,
            QuantityUnitOfMeasureId = material.QuantityUnitOfMeasureId,
            Name = material.Name,
            CreatedAt = material.CreatedAt,
            UpdatedAt = material.UpdatedAt,
        };
    }
}
