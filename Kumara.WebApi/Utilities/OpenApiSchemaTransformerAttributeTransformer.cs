// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Kumara.Utilities;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class OpenApiSchemaTransformerAttribute : Attribute
{
    public Type Type { get; }

    public OpenApiSchemaTransformerAttribute(Type type)
    {
        Type = type;
    }
}

public class OpenApiSchemaTransformerAttributeTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        List<Task> tasks = new List<Task>();

        Type contextType = context.JsonTypeInfo.Type;
        contextType = Nullable.GetUnderlyingType(contextType) ?? contextType;

        var attributes = (OpenApiSchemaTransformerAttribute[])
            contextType.GetCustomAttributes(
                typeof(OpenApiSchemaTransformerAttribute),
                inherit: true
            );

        foreach (var attribute in attributes)
        {
            var transformer = (IOpenApiSchemaTransformer)Activator.CreateInstance(attribute.Type)!;
            var task = transformer.TransformAsync(schema, context, cancellationToken);
            tasks.Add(task);
        }

        return Task.WhenAll(tasks);
    }
}
