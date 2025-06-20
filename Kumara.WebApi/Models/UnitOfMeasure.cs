// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations.Schema;

namespace Kumara.WebApi.Models;

public class UnitOfMeasure : ApplicationEntity
{
    public Guid Id { get; set; }

    public required Guid ITwinId { get; set; }

    public required string Name { get; set; }
    public required string Symbol { get; set; }
}
