// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using Kumara.Models;

namespace Kumara.WebApi.Controllers.Responses;

public class UnitOfMeasureResponse
{
    public Guid Id { get; set; }

    public required Guid ITwinId { get; set; }

    public required string Name { get; set; }

    public required string Symbol { get; set; }

    public static UnitOfMeasureResponse FromUnitOfMeasure(UnitOfMeasure uom)
    {
        return new UnitOfMeasureResponse
        {
            Id = uom.Id,
            ITwinId = uom.ITwinId,
            Name = uom.Name,
            Symbol = uom.Symbol,
        };
    }
}
