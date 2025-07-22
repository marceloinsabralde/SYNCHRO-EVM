// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Queries;
using Kumara.WebApi.Database;
using Kumara.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace Kumara.WebApi.Queries;

public class ListMaterialsQuery
    : IPageableQuery<ListMaterialsQuery, ListMaterialsQueryFilter, Material>
{
    private IQueryable<Material> _query;
    private int _limit = 50;

    public ListMaterialsQuery(IQueryable<Material> query, Guid iTwinId)
    {
        _query = query
            .AsNoTracking()
            .Where(material => material.ITwinId == iTwinId)
            .OrderBy(material => material.Id);
    }

    public ListMaterialsQuery ApplyFilter(ListMaterialsQueryFilter filter)
    {
        if (filter.ContinueFromId is not null)
            _query = _query.Where(material => material.Id > filter.ContinueFromId);

        return this;
    }

    public ListMaterialsQuery WithLimit(int limit)
    {
        _limit = limit;
        return this;
    }

    public QueryResult<Material> ExecuteQuery()
    {
        var items = _query.Take(_limit + 1).ToList();

        bool hasMore = items.Count > _limit;
        if (hasMore)
            items.RemoveAt(items.Count - 1);

        return new QueryResult<Material>() { Items = items, HasMore = hasMore };
    }
}

public class ListMaterialsQueryFilter : IPageableQueryFilter
{
    public Guid? ContinueFromId { get; set; }
}
