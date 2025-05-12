// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.Utilities;

namespace Kumara.EventSource.Models;

public class Channel
{
    public Guid Id { get; set; } = GuidUtility.CreateGuid();
    public required string Code { get; set; }
    public ICollection<EntityType> EntityTypes { get; set; } = [];
    public ICollection<EventType> EventTypes { get; set; } = [];
}
