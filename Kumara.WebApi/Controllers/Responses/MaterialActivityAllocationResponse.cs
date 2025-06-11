// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.WebApi.Models;
using NodaTime;

namespace Kumara.WebApi.Controllers.Responses;

public class MaterialActivityAllocationResponse
{
    public Guid Id { get; set; }

    public Guid ITwinId { get; set; }
    public Guid MaterialId { get; set; }
    public Guid ActivityId { get; set; }
    public Guid QuantityUnitOfMeasureId { get; set; }
    public decimal QuantityAtComplete { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }

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
            CreatedAt = allocation.CreatedAt,
            UpdatedAt = allocation.UpdatedAt,
        };
    }
}
