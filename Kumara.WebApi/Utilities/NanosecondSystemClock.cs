// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using NodaTime;

namespace Kumara.WebApi.Utilities;

class NanosecondSystemClock : IClock
{
    public static NanosecondSystemClock Instance { get; } = new();

    private NanosecondSystemClock() { }

    public Instant GetCurrentInstant()
    {
        var instant = SystemClock.Instance.GetCurrentInstant();

        var nanosecondFraction = Duration.FromTicks(instant.ToUnixTimeTicks() % 10);
        instant -= nanosecondFraction;

        return instant;
    }
}
