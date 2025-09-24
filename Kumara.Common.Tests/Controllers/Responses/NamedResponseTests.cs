// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Reflection;
using Kumara.Common.Controllers.Responses;
using Kumara.Common.Utilities;
using Swashbuckle.AspNetCore.Annotations;

namespace Kumara.Common.Tests.Controllers.Responses;

public class NamedResponseTests
{
    class TestResponse<T> : NamedResponse<T>;

    class CupOfTea;

    class PastryResponse;

    [Fact]
    public void JsonPropertyNamesInflectsOnGenericTypeName()
    {
        TestResponse<CupOfTea>.JsonPropertyNames.ShouldBe(
            new Dictionary<string, string> { { "item", "cupOfTea" }, { "items", "cupsOfTea" } }
        );

        TestResponse<PastryResponse>.JsonPropertyNames.ShouldBe(
            new Dictionary<string, string> { { "item", "pastry" }, { "items", "pastries" } }
        );
    }

    [Fact]
    public void ImplementsJsonPropertyNamesAttributes()
    {
        Type type = typeof(TestResponse<CupOfTea>);

        var jsonAttr = type.GetCustomAttributes<JsonTypeInfoResolverAttribute>(inherit: true)
            .FirstOrDefault();
        jsonAttr.ShouldNotBeNull();
        jsonAttr.Type.ShouldBe(typeof(JsonPropertyNamesTypeInfoResolver));

        var swaggerAttr = type.GetCustomAttributes<SwaggerSchemaFilterAttribute>(inherit: true)
            .FirstOrDefault();
        swaggerAttr.ShouldNotBeNull();
        swaggerAttr.Type.ShouldBe(typeof(JsonPropertyNamesSchemaPatcher));

        var openApiAttr = type.GetCustomAttributes<OpenApiSchemaTransformerAttribute>(inherit: true)
            .FirstOrDefault();
        openApiAttr.ShouldNotBeNull();
        openApiAttr.Type.ShouldBe(typeof(JsonPropertyNamesSchemaPatcher));
    }
}
