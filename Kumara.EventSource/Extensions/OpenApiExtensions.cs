// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Reflection;
using Kumara.EventSource.OpenApi;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Kumara.EventSource.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddEventSourceOpenApi(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerExamplesFromAssemblyOf<PostEventsRequestExample>();
        services.AddTransient<EventQueryParameterOperationFilter>();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = "SYNCHRO EVM EventSource API",
                    Version = "v1",
                    Description =
                        "API for event sourcing and persistence in the SYNCHRO EVM (Kumara) platform.",
                    Contact = new OpenApiContact
                    {
                        Name = "Bentley Systems",
                        Email = "support@bentley.com",
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Bentley Systems Proprietary",
                        Url = new Uri("https://www.bentley.com/legal/overview/"),
                    },
                }
            );

            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            c.OperationFilter<EventQueryParameterOperationFilter>();
            c.SchemaFilter<EventDataSchemaFilter>();
            c.ExampleFilters();

            c.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                }
            );
        });

        services.AddSwaggerGenNewtonsoftSupport();

        return services;
    }
}
