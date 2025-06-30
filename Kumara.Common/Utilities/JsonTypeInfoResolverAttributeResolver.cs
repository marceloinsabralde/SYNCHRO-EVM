// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Kumara.Common.Utilities;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class JsonTypeInfoResolverAttribute : Attribute
{
    public Type Type { get; }

    public JsonTypeInfoResolverAttribute(Type type)
    {
        Type = type;
    }
}

public class JsonTypeInfoResolverAttributeResolver : IJsonTypeInfoResolver
{
    public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var attributes = type.GetCustomAttributes<JsonTypeInfoResolverAttribute>(inherit: true);

        foreach (var attribute in attributes)
        {
            var resolver = (IJsonTypeInfoResolver)Activator.CreateInstance(attribute.Type)!;
            var result = resolver.GetTypeInfo(type, options);

            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }
}
