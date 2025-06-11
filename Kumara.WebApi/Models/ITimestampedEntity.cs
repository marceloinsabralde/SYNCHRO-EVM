// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using NodaTime;

namespace Kumara.WebApi.Models;

public interface ITimestampedEntity
{
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
}
