// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Utilities;
using Kumara.TestCommon.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

[assembly: AssemblyFixture(typeof(Kumara.Common.Tests.AppServicesHelperFixture))]

namespace Kumara.Common.Tests;

public class AppServicesHelperFixture : IDisposable
{
    public AppServicesHelperFixture()
    {
        AppServicesHelper.FallbackHost = CreateFallbackHost();
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
                        options.JsonSerializerOptions.TypeInfoResolverChain.Insert(
                            0,
                            new JsonTypeInfoResolverAttributeResolver()
                        );
                    });
                services.AddSwaggerGen(options =>
                {
                    options.UseAllOfToExtendReferenceSchemas();
                    options.EnableAnnotations();
                });
                services.AddOpenApi(options =>
                {
                    options.AddSchemaTransformer(
                        new OpenApiSchemaTransformerAttributeTransformer()
                    );
                });
            });
        });
        return builder.Build();
    }
}
