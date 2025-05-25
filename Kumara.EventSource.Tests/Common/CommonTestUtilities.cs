// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

namespace Kumara.EventSource.Tests.Common;

public static class CommonTestUtilities
{
    public static DateTimeOffset GetTestDateTimeOffset()
    {
        return new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
    }
}
