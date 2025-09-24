// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.WebApi.Types;
using NodaTime;

namespace Kumara.WebApi.Helpers;

public static class DateWithOptionalTimeHelper
{
    public static DateWithOptionalTime? GetFromBackingFields(
        ref Instant? backingDateTime,
        ref bool? backingHasTime
    )
    {
        if (backingDateTime is null)
            return null;

        return new DateWithOptionalTime()
        {
            DateTime = backingDateTime.Value.WithOffset(Offset.Zero),
            HasTime = backingHasTime.GetValueOrDefault(),
        };
    }

    public static void SetBackingFields(
        DateWithOptionalTime? value,
        ref Instant? backingDateTime,
        ref bool? backingHasTime
    )
    {
        backingDateTime = value?.DateTime.ToInstant();
        backingHasTime = value?.HasTime;
    }
}
