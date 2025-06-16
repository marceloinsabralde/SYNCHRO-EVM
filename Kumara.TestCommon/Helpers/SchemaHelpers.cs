// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Kumara.TestCommon.Helpers;

public class SchemaHelpers
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

    public static OpenApiSchema GenerateSwaggerSchema(Type type)
    {
        var schemaRepository = new SchemaRepository();
        AppServicesHelper.SwaggerSchemaGenerator.GenerateSchema(type, schemaRepository);
        var schema = schemaRepository.Schemas[type.Name];

        return schema;
    }

    public static async Task<OpenApiSchema> GenerateOpenApiSchemaAsync(Type type)
    {
        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureWebHost(webBuilder =>
        {
            webBuilder.UseTestServer();

            webBuilder.ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddOpenApi();
                services.AddSingleton<IOptionsMonitor<OpenApiOptions>>(
                    AppServicesHelper.OpenApiOptionsMonitor
                );
            });

            webBuilder.Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapOpenApi();
                    endpoints
                        .MapGet("/test", (HttpContext _) => Results.Ok(null))
                        .WithMetadata(
                            new ProducesResponseTypeMetadata(StatusCodes.Status200OK, type)
                        )
                        .WithOpenApi();
                });
            });
        });

        using var host = await builder.StartAsync(TestContext.Current.CancellationToken);
        var client = host.GetTestClient();

        var document = await SchemaHelpers.GetSchemaDocumentAsync(
            client,
            SchemaHelpers.OpenApiPath
        );
        var schema = document.Components.Schemas[type.Name];

        return schema;
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
