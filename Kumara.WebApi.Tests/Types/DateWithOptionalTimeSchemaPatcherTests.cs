// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.TestCommon.Helpers;
using Microsoft.OpenApi.Models;

namespace Kumara.WebApi.Tests.Types;

public class DateWithOptionalTimeSchemaPatcherTests : DatabaseTestBase
{
    protected OpenApiSchema ExpectedSchema
    {
        get
        {
            return new OpenApiSchema
            {
                AnyOf =
                [
                    new OpenApiSchema { Type = "string", Format = "date" },
                    new OpenApiSchema { Type = "string", Format = "date-time" },
                ],
                Format = "date-time",
            };
        }
    }

    [Fact]
    public async Task SwaggerSchemaHasOptionalTimeFormats()
    {
        var document = await SchemaHelpers.GetSchemaDocumentAsync(
            _client,
            SchemaHelpers.SwaggerPath
        );
        var actual = document.Components.Schemas["DateWithOptionalTime"];

        SchemaHelpers.Dump(actual).ShouldBe(SchemaHelpers.Dump(ExpectedSchema));
    }

    [Fact]
    public async Task SwaggerSchemaHasNullableOptionalTimeProperties()
    {
        var document = await SchemaHelpers.GetSchemaDocumentAsync(
            _client,
            SchemaHelpers.SwaggerPath
        );
        var allProperties = SchemaHelpers.GetAllPropertiesByPath(document).Values;
        var optionalTimeProperties = allProperties.Where(property =>
            property.AllOf.Count == 1
            && property.AllOf.All(refProperty =>
                refProperty.Reference?.Id == "DateWithOptionalTime"
            )
        );

        optionalTimeProperties.ShouldContain(property => property.Nullable);
    }

    [Fact]
    public async Task OpenApiSchemaHasNullableOptionalTimeProperties()
    {
        var document = await SchemaHelpers.GetSchemaDocumentAsync(
            _client,
            SchemaHelpers.OpenApiPath
        );
        var allProperties = SchemaHelpers.GetAllPropertiesByPath(document).Values;

        var expected = SchemaHelpers.ShallowClone(ExpectedSchema);
        expected.Nullable = true;

        allProperties
            .Select(actual => SchemaHelpers.Dump(actual))
            .ShouldContain(SchemaHelpers.Dump(expected));
    }
}
