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
}
