// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Bentley.ConnectCoreLibs.Providers;
using Kumara.Common.Options;
using Kumara.Common.Providers;
using Microsoft.Extensions.DependencyInjection;

public static class BentleyServicesExtensions
{
    const int DefaultWebClientTimeout = 5000;

    public static IServiceCollection AddBentleyDataProviders(this IServiceCollection services) =>
        services.ConfigureConnectCoreProviders().AddScoped<ItwinDataProvider, ItwinDataProvider>();

    public static IServiceCollection AddBuddi(
        this IServiceCollection services,
        BuddiOptions buddiOptions
    )
    {
        services.AddHttpClient(
            "Bentley.ConnectCoreLibs.BuddiUrlProviderClient",
            client =>
            {
                client.Timeout = TimeSpan.FromMilliseconds(DefaultWebClientTimeout);
            }
        );

        services.AddBuddiProvider(options =>
        {
            options.Uri = buddiOptions.Uri;
            options.Region = buddiOptions.Region;
            options.FallbackUris = buddiOptions.FallbackUris;
        });

        return services;
    }

    public static IServiceCollection ConfigureDelegation(
        this IServiceCollection services,
        OpenIdConnectOptions openIdConnectOptions
    )
    {
        services.AddHttpClient(
            "Bentley.ConnectCoreLibs.User.TokenExchangerClient",
            client =>
            {
                client.Timeout = TimeSpan.FromMilliseconds(DefaultWebClientTimeout);
            }
        );

        services.AddTokenExchangers(o =>
        {
            o.Authority = openIdConnectOptions.Authority;

            o.OidcTokenClientOptions.ClientId = openIdConnectOptions.DelegationClientId;
            o.OidcTokenClientOptions.ClientSecret = openIdConnectOptions.DelegationClientSecret;
        });

        return services;
    }
}
