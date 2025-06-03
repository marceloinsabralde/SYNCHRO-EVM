// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Kumara.EventSource.OpenApi;

/// <summary>
/// Operation filter to document the supported query parameters for the GET /events endpoint.
/// </summary>
public class EventQueryParameterOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        ApiDescription? apiDescription = context.ApiDescription;

        // Only apply to the GET events endpoint
        if (
            apiDescription.RelativePath == null
            || !apiDescription.RelativePath.EndsWith("events", StringComparison.OrdinalIgnoreCase)
            || apiDescription.HttpMethod?.ToUpperInvariant() != "GET"
        )
        {
            return;
        }

        // Add detailed descriptions of supported query parameters
        OpenApiParameter[] parameters = new[]
        {
            new OpenApiParameter
            {
                Name = "id",
                In = ParameterLocation.Query,
                Description = "Filter events by their unique identifier (Guid)",
                Required = false,
                Schema = new OpenApiSchema { Type = "string", Format = "uuid" },
            },
            new OpenApiParameter
            {
                Name = "iTwinId",
                In = ParameterLocation.Query,
                Description =
                    "Filter events by iTwin identifier (Guid) - Organizational digital twin of a construction project",
                Required = false,
                Schema = new OpenApiSchema { Type = "string", Format = "uuid" },
            },
            new OpenApiParameter
            {
                Name = "accountId",
                In = ParameterLocation.Query,
                Description = "Filter events by account identifier (Guid)",
                Required = false,
                Schema = new OpenApiSchema { Type = "string", Format = "uuid" },
            },
            new OpenApiParameter
            {
                Name = "correlationId",
                In = ParameterLocation.Query,
                Description = "Filter events by correlation ID",
                Required = false,
                Schema = new OpenApiSchema { Type = "string" },
            },
            new OpenApiParameter
            {
                Name = "type",
                In = ParameterLocation.Query,
                Description = "Filter events by type (e.g., 'control.account.created.v1')",
                Required = false,
                Schema = new OpenApiSchema { Type = "string" },
            },
            new OpenApiParameter
            {
                Name = "top",
                In = ParameterLocation.Query,
                Description = "Maximum number of events to return. Default is 50, maximum is 200.",
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = "integer",
                    Format = "int32",
                    Default = new OpenApiInteger(50),
                },
            },
            new OpenApiParameter
            {
                Name = "continuationtoken",
                In = ParameterLocation.Query,
                Description = "Continuation token for retrieving the next page of results",
                Required = false,
                Schema = new OpenApiSchema { Type = "string" },
            },
        };

        // Add the parameters to the operation
        foreach (OpenApiParameter parameter in parameters)
        {
            if (!operation.Parameters.Any(p => p.Name == parameter.Name && p.In == parameter.In))
            {
                operation.Parameters.Add(parameter);
            }
        }

        // Add pagination information to the response description
        if (operation.Responses.TryGetValue("200", out OpenApiResponse? response))
        {
            response.Description +=
                " Results are paginated, with navigation links provided in the response.";
        }
    }
}
