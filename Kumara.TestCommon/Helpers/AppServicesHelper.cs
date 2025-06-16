// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Kumara.TestCommon.Helpers;

public static class AppServicesHelper
{
    private static Type FindProgramEntryPoint()
    {
        return AppDomain
            .CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsClass && type.Name == "Program")
            .Single();
    }

    private static IDisposable CreateWebApplicationFactory(Type entryPoint)
    {
        var factoryType = typeof(WebApplicationFactory<>).MakeGenericType(entryPoint);
        var factoryInstance = Activator.CreateInstance(factoryType);
        var builderMethod = factoryType.GetMethod(
            "WithWebHostBuilder",
            new[] { typeof(Action<IWebHostBuilder>) }
        )!;

        Action<IWebHostBuilder> configureBuilder = builder =>
        {
            builder.UseEnvironment("Test");
            builder.ConfigureLogging(logging => logging.ClearProviders());
        };
        return (IDisposable)builderMethod.Invoke(factoryInstance, [configureBuilder])!;
    }

    private static readonly Lazy<IServiceProvider> _lazyServiceProvider = new(() =>
    {
        var entryPoint = FindProgramEntryPoint();
        var appFactory = CreateWebApplicationFactory(entryPoint);
        var servicesProperty = appFactory.GetType().GetProperty("Services")!;
        return (IServiceProvider)servicesProperty.GetValue(appFactory)!;
    });

    public static JsonSerializerOptions JsonSerializerOptions
    {
        get
        {
            var mvcOptions = _lazyServiceProvider.Value.GetRequiredService<
                IOptions<Microsoft.AspNetCore.Mvc.JsonOptions>
            >();
            var jsonOptions = mvcOptions.Value.JsonSerializerOptions;

            return jsonOptions;
        }
    }

    public static ISchemaGenerator SwaggerSchemaGenerator
    {
        get { return _lazyServiceProvider.Value.GetRequiredService<ISchemaGenerator>(); }
    }

    public static IOptionsMonitor<OpenApiOptions> OpenApiOptionsMonitor
    {
        get
        {
            return _lazyServiceProvider.Value.GetRequiredService<IOptionsMonitor<OpenApiOptions>>();
        }
    }
}
