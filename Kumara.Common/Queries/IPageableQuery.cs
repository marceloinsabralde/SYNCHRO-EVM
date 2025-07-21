// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Database;

namespace Kumara.Common.Queries;

public interface IPageableQuery<TQuery, TFilter, TPageableEntity>
    where TPageableEntity : IPageableEntity
    where TFilter : IPageableQueryFilter
{
    public TQuery ApplyFilter(TFilter filter);
    public TQuery WithLimit(int limit);
    public QueryResult<TPageableEntity> ExecuteQuery();
}
