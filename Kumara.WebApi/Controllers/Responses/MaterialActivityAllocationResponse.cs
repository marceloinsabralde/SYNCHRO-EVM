// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using Kumara.Models;

namespace Kumara.WebApi.Controllers.Responses;

public class MaterialActivityAllocationResponse
{
    public Guid Id { get; set; }

    public required Guid ITwinId { get; set; }
    public required Guid MaterialId { get; set; }
    public required Guid ActivityId { get; set; }
    public required Guid QuantityUnitOfMeasureId { get; set; }
    public required decimal QuantityAtComplete { get; set; }

    public static MaterialActivityAllocationResponse FromMaterialActivityAllocation(
        MaterialActivityAllocation allocation
    )
    {
        return new MaterialActivityAllocationResponse
        {
            Id = allocation.Id,
            ITwinId = allocation.ITwinId,
            ActivityId = allocation.ActivityId,
            MaterialId = allocation.MaterialId,
            QuantityUnitOfMeasureId = allocation.QuantityUnitOfMeasureId,
            QuantityAtComplete = allocation.QuantityAtComplete,
        };
    }
}
