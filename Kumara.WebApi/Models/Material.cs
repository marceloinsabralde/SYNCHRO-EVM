// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Database;

namespace Kumara.WebApi.Models;

public class Material : ApplicationEntity, IPageableEntity
{
    public Guid Id { get; set; }

    public Guid ITwinId { get; set; }

    public required string Name { get; set; }

    public Guid ResourceRoleId { get; set; }

    public Guid QuantityUnitOfMeasureId { get; set; }
    public required UnitOfMeasure QuantityUnitOfMeasure { get; set; }
}
