// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Linq.Expressions;
using System.Text.Json;
using Kumara.EventSource.Utilities;

namespace Kumara.EventSource.Models;

public class EventQueryBuilder
{
    private readonly List<Expression<Func<Event, bool>>> _predicates = new();
    private int _limit;
    private bool _hasLimit;

    public Dictionary<string, string> TokenQueryParameters { get; private set; } = new();

    public EventQueryBuilder WhereId(Guid id)
    {
        _predicates.Add(e => e.Id == id);
        return this;
    }

    public EventQueryBuilder WhereITwinGuid(Guid iTwinGuid)
    {
        _predicates.Add(e => e.ITwinGuid == iTwinGuid);
        return this;
    }

    public EventQueryBuilder WhereAccountGuid(Guid accountGuid)
    {
        _predicates.Add(e => e.AccountGuid == accountGuid);
        return this;
    }

    public EventQueryBuilder WhereCorrelationId(string correlationId)
    {
        _predicates.Add(e => e.CorrelationId == correlationId);
        return this;
    }

    public EventQueryBuilder WhereType(string type)
    {
        _predicates.Add(e => e.Type == type);
        return this;
    }

    public EventQueryBuilder WhereDataJson(Func<JsonDocument, bool> dataPredicate)
    {
        _predicates.Add(e => dataPredicate(e.DataJson));
        return this;
    }

    public EventQueryBuilder WithContinuationToken(string continuationToken)
    {
        Pagination.ContinuationToken? token = Pagination.ParseContinuationToken(continuationToken);
        if (token != null)
        {
            _predicates.Add(e => e.Id > token.Id);

            TokenQueryParameters = token.QueryParameters ?? new Dictionary<string, string>();
        }

        return this;
    }

    public EventQueryBuilder Limit(int limit)
    {
        _limit = Math.Max(1, limit);
        _hasLimit = true;
        return this;
    }

    public IQueryable<Event> ApplyTo(IQueryable<Event> query)
    {
        foreach (Expression<Func<Event, bool>>? predicate in _predicates)
        {
            query = query.Where(predicate);
        }

        query = query.OrderBy(e => e.Id);

        if (_hasLimit)
        {
            query = query.Take(_limit + 1);
        }

        return query;
    }
}
