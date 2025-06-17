// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.WebApi.Database;
using NodaTime;

namespace Kumara.WebApi.Models;

public abstract class ApplicationEntity : ITimestampedEntity
{
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
}
