// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kumara.Common.Extensions;

static class GenericApplicationBuilderExtensions
{
    // Makes a bound appsettings section available to DI and also for immediate use.
    //
    // Registers the config as concrete <T> as well as `IOptions<T>`, `IOptionsSnapshot<T>` and `IOptionsMonitor<T>`>
    public static T RegisterConfig<T>(this WebApplicationBuilder b)
        where T : class => b.BindValidated<T>(OptionsSection<T>());

    private static T BindValidated<T>(this WebApplicationBuilder builder, string p)
        where T : class => builder.Services.BindValidated<T>(builder.Configuration, p);

    private static T BindValidated<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        string p
    )
        where T : class
    {
        var boundConfig = configuration.GetSection(p);
        var config =
            boundConfig.Get<T>()
            ?? throw new InvalidOperationException($"{p} section missing from appsettings");

        // NOTE: `AddOptions` registers Services for `IOptions<T>`, `IOptionsSnapshot<T>` and `IOptionsMonitor<T>`
        services.AddOptions<T>().Bind(boundConfig).ValidateOnStart();

        // NOTE: And we register the concrete `T` as well, resolved via previous
        services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<T>>().Value);

        // NOTE: before you instantiate the builder into an application, you can't actually make use of the services you have configured so far.
        // you can call `BuildServiceProvider` but that ... builds the container early, just to immediately throw it away again, which seems silly
        // unfortunately, a bunch of code is written so that things that should be amenable to DI are not
        // (usually by passing an object that was not subject to DI to a constructor, e.g. `options = new BlahOptions; configure.Invoke(options); SomeConstructor(options);`)
        // in our case, hey we're reading appsettings, it's either from JSON or Env or some other source that is probably settled at boot
        // if you see this being used, it probably indicates a DI "issue" in downstream code
        return config;
    }

    private static string OptionsSection<T>() => typeof(T).Name.RemoveSuffix("Options");

    private static string RemoveSuffix(this string s, string suffix) =>
        s.EndsWith(suffix) ? s[..^suffix.Length] : s;
}
