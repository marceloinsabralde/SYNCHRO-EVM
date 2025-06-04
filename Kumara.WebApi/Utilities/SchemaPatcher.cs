// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Kumara.Utilities;

public abstract class SchemaPatcher : ISchemaFilter, IOpenApiSchemaTransformer
{
    protected abstract void Patch(OpenApiSchema schema);

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        Patch(schema);
    }

    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        Patch(schema);
        return Task.CompletedTask;
    }
}
