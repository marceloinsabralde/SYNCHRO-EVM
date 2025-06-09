// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using NodaTime.Text;

namespace Kumara.Converters;

public class InstantJsonConverter : ValueConverter<Instant, string>
{
    public InstantJsonConverter()
        : base(
            v => InstantPattern.ExtendedIso.Format(v),
            v => OffsetDateTimePattern.ExtendedIso.Parse(v).Value.ToInstant()
        ) { }
}
