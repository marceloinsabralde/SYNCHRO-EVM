// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Linq.Expressions;
using System.Text.Json;

namespace Kumara.EventSource.Models;

public class EventEntityQueryBuilder
{
    private readonly List<Expression<Func<EventEntity, bool>>> _predicates = new();

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

    public IQueryable<EventEntity> ApplyTo(IQueryable<EventEntity> query)
    {
        foreach (Expression<Func<EventEntity, bool>>? predicate in _predicates)
        {
            query = query.Where(predicate);
        }

        return query;
    }
}
