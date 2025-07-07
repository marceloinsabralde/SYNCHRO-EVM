// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kumara.TestCommon.Converters;

public class ObjectJsonStringValueConverter : ValueConverter<object, string>
{
    public ObjectJsonStringValueConverter()
        : base(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<object>(v, (JsonSerializerOptions?)null)!
        ) { }
}
