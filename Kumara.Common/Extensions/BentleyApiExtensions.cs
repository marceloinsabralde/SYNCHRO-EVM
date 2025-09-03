// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Bentley.ConnectCoreLibs.Middleware.Abstractions;
using Bentley.ConnectCoreLibs.Providers;
using Kumara.Common.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Kumara.Common.Extensions;

public static class BentleyApiExtensions
{
    // Configures Authentication as an OAuth Protected Resource with the Bentley IMS authority
    public static WebApplicationBuilder ConfigureBentleyProtectedApi(
        this WebApplicationBuilder builder
    )
    {
        var openIdConnectOptions = builder.RegisterConfig<OpenIdConnectOptions>();

        builder
            .Services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                sharedOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.Audience = openIdConnectOptions.ApiName;
                o.Authority = openIdConnectOptions.Authority;
                o.SaveToken = true;
                o.MapInboundClaims = false;
                o.TokenValidationParameters.ValidIssuers = openIdConnectOptions.ValidIssuers;
            });

        return builder;
    }

    // Configures authentication for the Swagger UI and JSON.
    //
    // Provides a browser OIDC flow so unauthenticated Users can access.
    // Allows access to the `.json` file via a shared secret to facilitate Scorecard Checklists.
    public static WebApplicationBuilder ConfigureBentleySwaggerAuthentication(
        this WebApplicationBuilder builder
    )
    {
        var swaggerLoginAuthentication =
            builder.RegisterConfig<SwaggerLoginAuthenticationOptions>();
        builder.RegisterConfig<SwaggerLoginMiddlewareOptions>();

        builder
            .Services.AddAuthentication()
            .AddSwaggerLoginAuthentication(o =>
            {
                o.Authority = swaggerLoginAuthentication.Authority;
                o.ClientId = swaggerLoginAuthentication.ClientId;
                o.CallbackPath = swaggerLoginAuthentication.CallbackPath;
            });

        return builder;
    }

    // Adds the SwaggerLoginMiddleware to actually enforce Authentication and initiate the browser-OIDC flow
    public static void UseBentleySwaggerAuthentication(this IApplicationBuilder app) =>
        app.UseContextualMiddleware<ContextualSwaggerLoginMiddleware>();
}
