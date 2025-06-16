// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Kumara.TestCommon.Helpers;

public static class AppServicesHelper
{
    public static IHost? FallbackHost { get; set; }

    private static Type? FindProgramEntryPoint()
    {
        return AppDomain
            .CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsClass && type.Name == "Program")
            .SingleOrDefault();
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

    public class AppFactory(IDisposable instance) : IDisposable
    {
        public HttpClient CreateClient()
        {
            var clientMethod = instance.GetType().GetMethod("CreateClient", Type.EmptyTypes)!;
            return (HttpClient)clientMethod.Invoke(instance, [])!;
        }

        public T GetRequiredService<T>()
            where T : notnull
        {
            var servicesProperty = instance.GetType().GetProperty("Services")!;
            var services = (IServiceProvider)servicesProperty.GetValue(instance)!;
            return services.GetRequiredService<T>();
        }

        public void Dispose()
        {
            instance.Dispose();
        }
    }

    public static AppFactory CreateWebApplicationFactory()
    {
        var entryPoint = FindProgramEntryPoint();
        if (entryPoint is null)
        {
            throw new InvalidOperationException("Cannot find program entry point.");
        }

        var appFactory = CreateWebApplicationFactory(entryPoint);

        return new AppFactory(appFactory);
    }

    private static readonly Lazy<IServiceProvider> _lazyServiceProvider = new(() =>
    {
        var entryPoint = FindProgramEntryPoint();
        if (entryPoint is not null)
        {
            var appFactory = CreateWebApplicationFactory(entryPoint);
            var servicesProperty = appFactory.GetType().GetProperty("Services")!;
            return (IServiceProvider)servicesProperty.GetValue(appFactory)!;
        }

        if (FallbackHost is not null)
        {
            return FallbackHost.Services;
        }

        throw new InvalidOperationException(
            "Cannot find program entry point and no FallbackHost configured."
        );
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
