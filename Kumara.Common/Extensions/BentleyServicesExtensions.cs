// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Bentley.CONNECT.Licensing.SaaS.Client.WebApiCore;
using Bentley.ConnectCoreLibs.Providers;
using Bentley.ConnectCoreLibs.User.Abstractions;
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

    public static IServiceCollection ConfigureConnectCoreProviders(this IServiceCollection services)
    {
        services.AddHttpClient(
            "Bentley.ConnectCoreLibs.iTwinProviderClient",
            client =>
            {
                client.Timeout = TimeSpan.FromMilliseconds(DefaultWebClientTimeout);
            }
        );
        services.AddITwinProvider();

        services.AddHttpClient(
            "Bentley.ConnectCoreLibs.RBACiTwinProviderClient",
            client =>
            {
                client.Timeout = TimeSpan.FromMilliseconds(DefaultWebClientTimeout);
            }
        );

        services.AddMemoryCache();
        services.AddDistributedMemoryCache();
        services.AddRbacITwinProvider(options =>
        {
            // NOTE: Setting this will cause RBAC queries to filter to only this GprId
            options.GprId = "";
        });

        return services;
    }

    public static IServiceCollection ConfigureEntitlementWorkflow(this IServiceCollection services)
    {
        services.AddEntitlementWorkflowWithServiceProvider(
            (options, provider) =>
            {
                var buddi = provider.GetRequiredService<BuddiOptions>();
                options.BuddiRegionCode = buddi.Region;
                options.TokenType = AccessTokenType.JWT;
                options.PolicyCacheTtl = TimeSpan.FromMinutes(30);

                options.TokenFactory = async () =>
                {
                    const string entitlementPolicyServiceScope = "entitlement-policy-service-2576";
                    var exchanger = provider.GetRequiredService<ITokenExchanger>();
                    var token = await exchanger.ExchangeAsync(entitlementPolicyServiceScope);
                    return (token, TimeSpan.FromMinutes(5));
                };
            }
        );

        return services;
    }
}
