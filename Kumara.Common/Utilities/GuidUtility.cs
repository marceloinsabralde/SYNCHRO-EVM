// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

namespace Kumara.Common.Utilities;

public static class GuidUtility
{
    public static Guid CreateGuid(TimeProvider? timeProvider = null)
    {
        timeProvider ??= TimeProvider.System;
        DateTimeOffset timestamp = timeProvider.GetUtcNow();
        return Guid.CreateVersion7(timestamp);
    }
}
