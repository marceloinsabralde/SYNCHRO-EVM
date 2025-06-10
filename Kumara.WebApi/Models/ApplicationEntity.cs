// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using NodaTime;

namespace Kumara.Models;

public abstract class ApplicationEntity : ITimestampedEntity
{
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
}
