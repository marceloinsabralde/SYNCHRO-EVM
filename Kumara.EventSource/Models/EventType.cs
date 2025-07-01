// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Utilities;

namespace Kumara.EventSource.Models;

public class EventType
{
    public Guid Id { get; set; } = GuidUtility.CreateGuid();
    public required string Action { get; set; }
    public required string Version { get; set; }
    public required string Schema { get; set; }
    public required Guid ChannelId { get; set; }
    public required Guid EntityTypeId { get; set; }
    public Channel? Channel { get; set; }
    public EntityType? EntityType { get; set; }
    public ICollection<Event> Events { get; set; } = [];
}
