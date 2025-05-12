// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Linq.Expressions;
using System.Text.Json;
using Kumara.EventSource.Utilities;

namespace Kumara.EventSource.Models;

public class EventEntityQueryBuilder
{
    private readonly List<Expression<Func<EventEntity, bool>>> _predicates = new();
    private int _limit;
    private bool _hasLimit;

    public Dictionary<string, string> TokenQueryParameters { get; private set; } = new();

    public EventEntityQueryBuilder WhereId(Guid id)
    {
        _predicates.Add(e => e.Id == id);
        return this;
    }

    public EventEntityQueryBuilder WhereITwinGuid(Guid iTwinGuid)
    {
        _predicates.Add(e => e.ITwinGuid == iTwinGuid);
        return this;
    }

    public EventEntityQueryBuilder WhereAccountGuid(Guid accountGuid)
    {
        _predicates.Add(e => e.AccountGuid == accountGuid);
        return this;
    }

    public EventEntityQueryBuilder WhereCorrelationId(string correlationId)
    {
        _predicates.Add(e => e.CorrelationId == correlationId);
        return this;
    }

    public EventEntityQueryBuilder WhereType(string type)
    {
        _predicates.Add(e => e.Type == type);
        return this;
    }

    public EventEntityQueryBuilder WhereDataJson(Func<JsonDocument, bool> dataPredicate)
    {
        _predicates.Add(e => dataPredicate(e.DataJson));
        return this;
    }

    public EventEntityQueryBuilder WithContinuationToken(string continuationToken)
    {
        Pagination.ContinuationToken? token = Pagination.ParseContinuationToken(continuationToken);
        if (token != null)
        {
            _predicates.Add(e => e.Id > token.Id);

            TokenQueryParameters = token.QueryParameters ?? new Dictionary<string, string>();
        }

        return this;
    }

    public EventEntityQueryBuilder Limit(int limit)
    {
        _limit = Math.Max(1, limit);
        _hasLimit = true;
        return this;
    }

    public IQueryable<EventEntity> ApplyTo(IQueryable<EventEntity> query)
    {
        foreach (Expression<Func<EventEntity, bool>>? predicate in _predicates)
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
