// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

namespace Kumara.Common.Utilities;

using Microsoft.OpenApi.Models;
using NodaTime;

public class NodaTimeSchemaPatcher : SchemaPatcher
{
    protected override void Patch(OpenApiSchema schema, Type type)
    {
        if (type == typeof(Instant))
        {
            Clear(schema);
            schema.Type = "string";
            schema.Format = "date-time";
        }
        else if (type == typeof(LocalDate))
        {
            Clear(schema);
            schema.Type = "string";
            schema.Format = "date";
        }
    }
}
