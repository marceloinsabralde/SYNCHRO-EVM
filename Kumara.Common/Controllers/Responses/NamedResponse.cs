// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.RegularExpressions;
using Kumara.Common.Utilities;
using Swashbuckle.AspNetCore.Annotations;

namespace Kumara.Common.Controllers.Responses;

[JsonTypeInfoResolver(typeof(JsonPropertyNamesTypeInfoResolver))]
[SwaggerSchemaFilter(typeof(JsonPropertyNamesSchemaPatcher))]
[OpenApiSchemaTransformer(typeof(JsonPropertyNamesSchemaPatcher))]
public class NamedResponse<T>
{
    private static string BaseTypeName
    {
        get
        {
            var typeName = typeof(T).Name;
            var baseName = Regex.Replace(typeName, "Response$", "");
            return baseName;
        }
    }

    public static IDictionary<string, string> JsonPropertyNames
    {
        get
        {
            return new Dictionary<string, string>
            {
                { "item", Inflector.Camelize(BaseTypeName) },
                { "items", Inflector.Camelize(Inflector.Pluralize(BaseTypeName)) },
            };
        }
    }
}
