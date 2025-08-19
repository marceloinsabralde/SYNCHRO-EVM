// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Queries;
using Kumara.WebApi.Models;

namespace Kumara.WebApi.Queries;

public class ListMaterialsQuery
    : PageableQuery<ListMaterialsQuery, ListMaterialsQueryFilter, Material>
{
    public ListMaterialsQuery(IQueryable<Material> query, Guid iTwinId)
        : base(query)
    {
        _query = _query
            .Where(material => material.ITwinId == iTwinId)
            .OrderBy(material => material.Id);
    }

    public override ListMaterialsQuery ApplyFilter(ListMaterialsQueryFilter filter)
    {
        if (filter.ContinueFromId is not null)
            _query = _query.Where(material => material.Id > filter.ContinueFromId);

        return this;
    }
}

public class ListMaterialsQueryFilter : IPageableQueryFilter
{
    public Guid? ContinueFromId { get; set; }
}
