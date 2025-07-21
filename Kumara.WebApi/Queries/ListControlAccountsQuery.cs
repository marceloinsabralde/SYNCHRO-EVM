// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Queries;
using Kumara.WebApi.Database;
using Kumara.WebApi.Models;

namespace Kumara.WebApi.Queries;

public class ListControlAccountsQuery
    : IPageableQuery<ListControlAccountsQuery, ListControlAccountsQueryFilter, ControlAccount>
{
    private IQueryable<ControlAccount> _query;
    private int _limit = 50;

    public ListControlAccountsQuery(ApplicationDbContext dbContext, Guid iTwinId)
    {
        _query = dbContext
            .ControlAccounts.Where(controlAccount => controlAccount.ITwinId == iTwinId)
            .OrderBy(controlAccount => controlAccount.Id);
    }

    public ListControlAccountsQuery ApplyFilter(ListControlAccountsQueryFilter filter)
    {
        if (filter.ContinueFromId is not null)
            _query = _query.Where(controlAccount => controlAccount.Id > filter.ContinueFromId);

        return this;
    }

    public ListControlAccountsQuery WithLimit(int limit)
    {
        _limit = limit;
        return this;
    }

    public QueryResult<ControlAccount> ExecuteQuery()
    {
        var items = _query.Take(_limit + 1).ToList();

        bool hasMore = items.Count > _limit;
        if (hasMore)
            items.RemoveAt(items.Count - 1);

        return new QueryResult<ControlAccount>() { Items = items, HasMore = hasMore };
    }
}

public class ListControlAccountsQueryFilter : IPageableQueryFilter
{
    public Guid? ContinueFromId { get; set; }
}
