// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kumara.WebApi.Tests;

public static class AppServicesHelper
{
    private static readonly Lazy<IServiceProvider> _lazyServiceProvider = new(() =>
    {
        var appFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");
            builder.ConfigureLogging(logging => logging.ClearProviders());
        });
        return appFactory.Services;
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
}
