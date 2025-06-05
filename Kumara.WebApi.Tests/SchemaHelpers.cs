// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Kumara.WebApi.Tests;

class SchemaHelpers
{
    public const string SwaggerPath = "/swagger/v1/swagger.json";
    public const string OpenApiPath = "/openapi/v1.json";

    public static async Task<OpenApiDocument> GetSchemaDocumentAsync(HttpClient client, string path)
    {
        var response = await client.GetAsync(path, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var document = new OpenApiStreamReader().Read(
            response.Content.ReadAsStream(TestContext.Current.CancellationToken),
            out var diagnostic
        );

        return document;
    }

    public static OpenApiSchema ShallowClone(OpenApiSchema schema)
    {
        OpenApiSchema clone = new OpenApiSchema();

        foreach (var property in schema.GetType().GetProperties())
        {
            property.SetValue(clone, property.GetValue(schema));
        }

        return clone;
    }

    public static string Dump(IOpenApiSerializable element)
    {
        var schema = element as OpenApiSchema;
        if (schema is not null)
        {
            schema = ShallowClone(schema);
            schema.Reference = null;
            element = schema;
        }

        return element.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
    }

    public static IDictionary<string, OpenApiSchema> GetAllPropertiesByPath(
        OpenApiDocument document
    )
    {
        var properties = new Dictionary<string, OpenApiSchema>();

        foreach (var schemaItem in document.Components.Schemas)
        {
            var itemProperties = GetAllPropertiesByPath(schemaItem.Value, schemaItem.Key);
            foreach (var item in itemProperties)
            {
                properties.Add(item.Key, item.Value);
            }
        }

        return properties;
    }

    public static IDictionary<string, OpenApiSchema> GetAllPropertiesByPath(
        OpenApiSchema schema,
        string basePath
    )
    {
        var result = new Dictionary<string, OpenApiSchema>();

        result.Add(basePath, schema);

        if (schema.Properties is not null)
        {
            foreach (var thisItem in schema.Properties)
            {
                var itemPath = $"{basePath}.{thisItem.Key}";
                foreach (var childItem in GetAllPropertiesByPath(thisItem.Value, itemPath))
                {
                    result.Add(childItem.Key, childItem.Value);
                }
            }
        }

        return result;
    }
}
