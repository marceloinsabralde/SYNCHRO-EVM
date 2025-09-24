// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel;

namespace Kumara.WebApi.Types;

public record Settings
{
    [DefaultValue(false)]
    public required bool ActualsHaveTime { get; set; }
}
