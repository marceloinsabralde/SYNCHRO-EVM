// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Kumara.Common.Utilities;

public class JsonPropertyNamesSchemaPatcher : SchemaPatcher
{
    protected static IDictionary<string, string> GetMapping(Type type)
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

        return mapping;
    }

    protected override void Patch(OpenApiSchema schema, Type type)
    {
        var mapping = GetMapping(type);

        foreach (var (oldName, newName) in mapping)
        {
            if (schema.Properties.TryGetValue(oldName, out var property))
            {
                if (schema.Properties.ContainsKey(newName))
                {
                    throw new ArgumentException($"{type.Name}.{newName} already exists");
                }

                schema.Properties.Remove(oldName);
                schema.Properties.Add(newName, property);
            }

            if (schema.Required.Remove(oldName))
            {
                schema.Required.Add(newName);
            }
        }
    }

    public override Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var mapping = GetMapping(context.JsonTypeInfo.Type);

        foreach (var prop in context.JsonTypeInfo.Properties)
        {
            if (mapping.TryGetValue(prop.Name, out var newName))
            {
                var nameField = prop.GetType()
                    .BaseType!.GetField(
                        "_name",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly
                    )!;
                nameField.SetValue(prop, newName);
            }
        }

        return base.TransformAsync(schema, context, cancellationToken);
    }
}
