// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Utilities;

namespace Kumara.EventSource.Models;

public class EntityType
{
    public Guid Id { get; set; } = GuidUtility.CreateGuid();
    public required string Code { get; set; }
    public required Guid ChannelId { get; set; }
    public Channel? Channel { get; set; }
    public ICollection<EventType> EventTypes { get; set; } = [];
}
