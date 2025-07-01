// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Utilities;

namespace Kumara.EventSource.Models;

public class Client
{
    public Guid Id { get; set; } = GuidUtility.CreateGuid();
    public required string IMSClientId { get; set; }
    public required Guid GPRID { get; set; }
    public ICollection<ReadAuthority> ReadAuthorities { get; set; } = [];
    public ICollection<PublishAuthority> PublishAuthorities { get; set; } = [];
}
