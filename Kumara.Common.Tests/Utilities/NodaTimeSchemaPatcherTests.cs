// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.TestCommon.Helpers;
using Microsoft.OpenApi.Models;
using NodaTime;

namespace Kumara.Common.Tests.Utilities;

public class NodaTimeSchemaPatcherTests
{
    public OpenApiSchema DateTimeSchema = new()
    {
        Type = "string",
        Format = "date-time",
        Example = null,
    };

    [Fact]
    public void SwaggerSchemaHasInstantSchema()
    {
        var (schema, _) = SchemaHelpers.GenerateSwaggerSchema(typeof(Instant));
        SchemaHelpers.Dump(schema).ShouldBe(SchemaHelpers.Dump(DateTimeSchema));
    }

    [Fact]
    public async Task OpenApiSchemaHasInstantSchema()
    {
        var (schema, _) = await SchemaHelpers.GenerateOpenApiSchemaAsync(typeof(Instant));
        SchemaHelpers.Dump(schema).ShouldBe(SchemaHelpers.Dump(DateTimeSchema));
    }
}
