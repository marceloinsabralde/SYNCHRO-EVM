// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

namespace Kumara.Common.Queries;

public interface IPageableQueryResult
{
    public bool HasMore { get; init; }
    public Guid LastReadId { get; }
}
