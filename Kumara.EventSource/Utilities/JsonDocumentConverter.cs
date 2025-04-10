// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kumara.EventSource.Utilities;

public class JsonDocumentConverter : ValueConverter<JsonDocument, string>
{
    public JsonDocumentConverter()
        : base(
            v => JsonSerializer.Serialize(v, new JsonSerializerOptions { WriteIndented = false }),
            v => JsonDocument.Parse(string.IsNullOrEmpty(v) ? "{}" : v, new JsonDocumentOptions())
        ) { }
}
