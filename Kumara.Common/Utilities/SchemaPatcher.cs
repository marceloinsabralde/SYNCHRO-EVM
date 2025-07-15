// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Kumara.Common.Utilities;

public abstract class SchemaPatcher : ISchemaFilter, IOpenApiSchemaTransformer
{
    protected abstract void Patch(OpenApiSchema schema, Type type);

    public virtual void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        Patch(schema, context.Type);
    }

    public virtual Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        Patch(schema, context.JsonTypeInfo.Type);
        return Task.CompletedTask;
    }

    public static void Clear(OpenApiSchema schema)
    {
        OpenApiSchema defaultSchema = new OpenApiSchema();

        foreach (var property in schema.GetType().GetProperties())
        {
            property.SetValue(schema, property.GetValue(defaultSchema));
        }
    }
}
