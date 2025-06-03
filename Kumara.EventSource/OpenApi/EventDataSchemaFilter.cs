// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSource.Models;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Kumara.EventSource.OpenApi;

/// <summary>
/// Schema filter that ensures the Event.DataJson property is properly displayed as "data" in Swagger.
/// This filter transforms the JsonDocument property into a more user-friendly object representation.
/// </summary>
public class EventDataSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        // Only apply this filter to Event type
        if (context.Type != typeof(Event))
        {
            return;
        }

        // Update or add data property
        var dataSchema = new OpenApiSchema
        {
            Type = "object",
            Description =
                "Event-specific data payload. Each event type has its own schema for this object. "
                + "The schema matches event type naming convention: \n"
                + "- For 'control.account.created.v1': See control.account.created.v1.schema.json\n"
                + "- For 'control.account.updated.v1': See control.account.updated.v1.schema.json\n"
                + "- For 'activity.created.v1': See activity.created.v1.schema.json",
            Example = new OpenApiObject
            {
                ["id"] = new OpenApiString(Guid.NewGuid().ToString()),
                ["name"] = new OpenApiString("My Activity"),
                ["referenceCode"] = new OpenApiString("ACT-001"),
                ["wbsPath"] = new OpenApiString("1.2.3"),
                ["taskId"] = new OpenApiString(Guid.NewGuid().ToString()),
                ["plannedStart"] = new OpenApiString(DateTimeOffset.Now.ToString("o")),
                ["eventTypeVersion"] = new OpenApiString("1.0"),
            },
        };

        // Remove JsonDocument property and add generic data object
        schema.Properties.Remove("dataJson");
        schema.Properties["data"] = dataSchema;
    }
}
