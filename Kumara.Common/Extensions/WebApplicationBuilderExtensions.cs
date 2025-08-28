// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kumara.Common.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void ConfigureBentleyProtectedApi(
        this WebApplicationBuilder webApplicationBuilder
    )
    {
        webApplicationBuilder
            .Services.AddOptions<OpenIdConnectOptions>()
            .Bind(webApplicationBuilder.Configuration.GetSection("OpenIdConnect"))
            .ValidateOnStart();

        webApplicationBuilder
            .Services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                sharedOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                var openIdConnect = webApplicationBuilder
                    .Configuration.GetSection("OpenIdConnect")
                    .Get<OpenIdConnectOptions>()!;
                o.Audience = openIdConnect.ApiName;
                o.Authority = openIdConnect.Authority;
                o.SaveToken = true;
                o.MapInboundClaims = false;
                o.TokenValidationParameters.ValidIssuers = openIdConnect.ValidIssuers;
            });
    }
}
