// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Queries;
using Kumara.WebApi.Database;
using Kumara.WebApi.Models;

namespace Kumara.WebApi.Queries;

public class ListMaterialActivityAllocationsQuery
    : IPageableQuery<
        ListMaterialActivityAllocationsQuery,
        ListMaterialActivityAllocationsQueryFilter,
        MaterialActivityAllocation
    >
{
    private IQueryable<MaterialActivityAllocation> _query;
    private int _limit = 50;

    public ListMaterialActivityAllocationsQuery(
        IQueryable<MaterialActivityAllocation> query,
        Guid iTwinId
    )
    {
        _query = query
            .Where(allocation => allocation.ITwinId == iTwinId)
            .OrderBy(allocation => allocation.Id);
    }

    public ListMaterialActivityAllocationsQuery ApplyFilter(
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

    public ListMaterialActivityAllocationsQuery WithLimit(int limit)
    {
        _limit = limit;
        return this;
    }

    public QueryResult<MaterialActivityAllocation> ExecuteQuery()
    {
        var items = _query.Take(_limit + 1).ToList();

        bool hasMore = items.Count > _limit;
        if (hasMore)
            items.RemoveAt(items.Count - 1);

        return new QueryResult<MaterialActivityAllocation>() { Items = items, HasMore = hasMore };
    }
}

public record ListMaterialActivityAllocationsQueryFilter : IPageableQueryFilter
{
    public Guid? ContinueFromId { get; set; }
    public Guid? ActivityId { get; set; }
    public Guid? MaterialId { get; set; }
}
