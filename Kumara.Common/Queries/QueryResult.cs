// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Database;

namespace Kumara.Common.Queries;

public record QueryResult<T> : IPageableQueryResult
    where T : IPageableEntity
{
    public required List<T> Items;
    public bool HasMore { get; init; }
    public Guid LastReadId => Items.Last().Id;
}
