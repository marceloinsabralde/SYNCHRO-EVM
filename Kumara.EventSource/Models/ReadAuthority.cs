// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.Utilities;

namespace Kumara.EventSource.Models;

public class ReadAuthority
{
    public Guid Id { get; set; } = GuidUtility.CreateGuid();
    public required Guid ClientId { get; set; }
    public required Guid ChannelId { get; set; }
    public Client? Client { get; set; }
    public Channel? Channel { get; set; }
}
