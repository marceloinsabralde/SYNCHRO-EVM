// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Queries;
using Kumara.WebApi.Database;
using Kumara.WebApi.Models;

namespace Kumara.WebApi.Queries;

public class ListMaterialActivityAllocationsQuery
    : PageableQuery<
        ListMaterialActivityAllocationsQuery,
        ListMaterialActivityAllocationsQueryFilter,
        MaterialActivityAllocation
    >
{
    public ListMaterialActivityAllocationsQuery(
        IQueryable<MaterialActivityAllocation> query,
        Guid iTwinId
    )
        : base(query)
    {
        _query = _query
            .Where(allocation => allocation.ITwinId == iTwinId)
            .OrderBy(allocation => allocation.Id);
    }

    public override ListMaterialActivityAllocationsQuery ApplyFilter(
        ListMaterialActivityAllocationsQueryFilter filter
    )
    {
        if (filter.ContinueFromId is not null)
            _query = _query.Where(allocation => allocation.Id > filter.ContinueFromId);

        if (filter.ActivityId is not null)
            _query = _query.Where(allocation => allocation.ActivityId == filter.ActivityId);

        if (filter.MaterialId is not null)
            _query = _query.Where(allocation => allocation.MaterialId == filter.MaterialId);

        return this;
    }
}

public record ListMaterialActivityAllocationsQueryFilter : IPageableQueryFilter
{
    public Guid? ContinueFromId { get; set; }
    public Guid? ActivityId { get; set; }
    public Guid? MaterialId { get; set; }
}
