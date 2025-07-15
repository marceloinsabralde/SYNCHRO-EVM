// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Extensions;
using Kumara.TestCommon.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

[assembly: AssemblyFixture(typeof(Kumara.Common.Tests.AppServicesHelperFixture))]

namespace Kumara.Common.Tests;

public class AppServicesHelperFixture : IDisposable
{
    public AppServicesHelperFixture()
    {
        AppServicesHelper.FallbackHostFactory = CreateFallbackHost;
    }

    public void Dispose() { }

    private static IHost CreateFallbackHost()
    {
        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureWebHost(webBuilder =>
        {
            webBuilder.ConfigureServices(services =>
            {
                services
                    .AddControllers()
                    .AddJsonOptions(options =>
                    {
                        options.UseKumaraCommon();
                    });
                services.AddSwaggerGen(options =>
                {
                    options.UseKumaraCommon();
                });
                services.AddOpenApi(options =>
                {
                    options.UseKumaraCommon();
                });
            });
        });
        return builder.Build();
    }
}
