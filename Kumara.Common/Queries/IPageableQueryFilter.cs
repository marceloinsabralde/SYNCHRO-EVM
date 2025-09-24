// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

namespace Kumara.Common.Queries;

public interface IPageableQueryFilter
{
    public Guid? ContinueFromId { get; set; }
}
