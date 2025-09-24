// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Queries;
using Kumara.WebApi.Models;

namespace Kumara.WebApi.Queries;

public class ListUnitsOfMeasureQuery
    : PageableQuery<ListUnitsOfMeasureQuery, ListUnitsOfMeasureQueryFilter, UnitOfMeasure>
{
    public ListUnitsOfMeasureQuery(IQueryable<UnitOfMeasure> query, Guid iTwinId)
        : base(query)
    {
        _query = _query.Where(uom => uom.ITwinId == iTwinId).OrderBy(uom => uom.Id);
    }

    public override ListUnitsOfMeasureQuery ApplyFilter(ListUnitsOfMeasureQueryFilter filter)
    {
        if (filter.ContinueFromId is not null)
            _query = _query.Where(uom => uom.Id > filter.ContinueFromId);

        return this;
    }
}

public class ListUnitsOfMeasureQueryFilter : IPageableQueryFilter
{
    public Guid? ContinueFromId { get; set; }
}
