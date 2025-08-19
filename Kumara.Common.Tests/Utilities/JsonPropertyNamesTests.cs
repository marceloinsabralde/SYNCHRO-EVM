// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.Common.Utilities;
using Kumara.TestCommon.Helpers;
using Swashbuckle.AspNetCore.Annotations;

namespace Kumara.Common.Tests.Utilities;

public class JsonPropertyNamesTests
{
    [JsonTypeInfoResolver(typeof(JsonPropertyNamesTypeInfoResolver))]
    [SwaggerSchemaFilter(typeof(JsonPropertyNamesSchemaPatcher))]
    [OpenApiSchemaTransformer(typeof(JsonPropertyNamesSchemaPatcher))]
    struct TestEntity
    {
        public required string FirstName { get; set; }
        public string SecondName { get; set; }
        public required string ThirdName { get; set; }

        public static Dictionary<string, string> JsonPropertyNames =>
            new()
            {
                { "firstName", "someOtherName" },
                { "secondName", "yetAnotherName" },
                { "missing", "thisIsIgnored" },
            };
    }

    [Fact]
    public void SupportsSettingJsonPropertyNamesAtRuntime()
    {
        var originalEntity = new TestEntity
        {
            FirstName = "foo",
            SecondName = "bar",
            ThirdName = "baz",
        };
        var expectedData = new Dictionary<string, string>
        {
            { "someOtherName", "foo" },
            { "yetAnotherName", "bar" },
            { "thirdName", "baz" },
        };

        string actualJson = JsonSerializer.Serialize(
            originalEntity,
            AppServicesHelper.JsonSerializerOptions
        );
        var actualData = JsonSerializer.Deserialize<Dictionary<string, string>>(actualJson);
        actualData.ShouldBe(expectedData);

        var reconstitutedEntity = JsonSerializer.Deserialize<TestEntity>(
            actualJson,
            AppServicesHelper.JsonSerializerOptions
        );
        reconstitutedEntity.ShouldBe(originalEntity);
    }

    [Fact]
    public void SwaggerSchemaHasCustomPropertyNames()
    {
        var (schema, _) = SchemaHelpers.GenerateSwaggerSchema(typeof(TestEntity));

        schema.Properties.Keys.ShouldBe(
            ["someOtherName", "yetAnotherName", "thirdName"],
            ignoreOrder: true
        );
        schema.Required.ShouldBe(["someOtherName", "thirdName"], ignoreOrder: true);
    }

    [Fact]
    public void SwaggerSchemaInvokesPatchersForAllProperties()
    {
        var (_, visits) = SchemaHelpers.GenerateSwaggerSchema(typeof(TestEntity));

        visits
            .Select(visit => visit.Type)
            .ShouldBe(
                [typeof(TestEntity), typeof(string), typeof(string), typeof(string)],
                ignoreOrder: true
            );
    }

    [Fact]
    public async Task OpenApiSchemaHasCustomPropertyNames()
    {
        var (schema, _) = await SchemaHelpers.GenerateOpenApiSchemaAsync(typeof(TestEntity));

        schema.Properties.Keys.ShouldBe(
            ["someOtherName", "yetAnotherName", "thirdName"],
            ignoreOrder: true
        );
        schema.Required.ShouldBe(["someOtherName", "thirdName"], ignoreOrder: true);
    }

    [Fact]
    public async Task OpenApiSchemaInvokesPatchersForAllProperties()
    {
        var (_, visits) = await SchemaHelpers.GenerateOpenApiSchemaAsync(typeof(TestEntity));

        visits
            .Select(visit => visit.Type)
            .ShouldBe(
                [typeof(TestEntity), typeof(string), typeof(string), typeof(string)],
                ignoreOrder: true
            );
    }
}
