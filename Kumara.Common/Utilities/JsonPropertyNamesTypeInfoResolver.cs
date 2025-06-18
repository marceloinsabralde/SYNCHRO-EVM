// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Kumara.Common.Utilities;

public class JsonPropertyNamesTypeInfoResolver : IJsonTypeInfoResolver
{
    public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var mapping =
            type.GetProperty(
                    "JsonPropertyNames",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy
                )
                ?.GetValue(null) as IDictionary<string, string>;
        if (mapping is null)
        {
            throw new ArgumentException($"{type.Name}.JsonPropertyNames must return a mapping");
        }

        var defaultResolver = JsonSerializerOptions.Default.TypeInfoResolver!;
        var typeInfo = defaultResolver.GetTypeInfo(type, options)!;

        foreach (JsonPropertyInfo property in typeInfo.Properties)
        {
            if (mapping.TryGetValue(property.Name, out var newName))
            {
                property.Name = newName;
            }
        }

        return typeInfo;
    }
}
