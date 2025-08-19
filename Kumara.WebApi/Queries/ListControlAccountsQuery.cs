// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Queries;
using Kumara.WebApi.Models;

namespace Kumara.WebApi.Queries;

public class ListControlAccountsQuery
    : PageableQuery<ListControlAccountsQuery, ListControlAccountsQueryFilter, ControlAccount>
{
    public ListControlAccountsQuery(IQueryable<ControlAccount> query, Guid iTwinId)
        : base(query)
    {
        _query = _query
            .Where(controlAccount => controlAccount.ITwinId == iTwinId)
            .OrderBy(controlAccount => controlAccount.Id);
    }

    public override ListControlAccountsQuery ApplyFilter(ListControlAccountsQueryFilter filter)
    {
        if (filter.ContinueFromId is not null)
            _query = _query.Where(controlAccount => controlAccount.Id > filter.ContinueFromId);

        return this;
    }
}

public class ListControlAccountsQueryFilter : IPageableQueryFilter
{
    public Guid? ContinueFromId { get; set; }
}
