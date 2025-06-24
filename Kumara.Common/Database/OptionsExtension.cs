// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Utilities;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;

namespace Kumara.Common.Database;

public class OptionsExtension(IServiceProvider? serviceProvider) : IDbContextOptionsExtension
{
    public IServiceProvider? ServiceProvider => serviceProvider;

    private DbContextOptionsExtensionInfo? _info;
    public DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);

    public void ApplyServices(IServiceCollection services)
    {
        var clock = serviceProvider?.GetService<IClock>() ?? NanosecondSystemClock.Instance;
        services.TryAddSingleton<IClock>(clock);

        services.AddSingleton<IInterceptor, TimestampedEntityInterceptor>();
        services.AddSingleton<IInterceptor, ValidateChangesInterceptor>();

        services.AddSingleton<IConventionSetPlugin, ConventionSetPlugin>();
    }

    public void Validate(IDbContextOptions options) { }

    private class ExtensionInfo(IDbContextOptionsExtension extension)
        : DbContextOptionsExtensionInfo(extension)
    {
        public override bool IsDatabaseProvider => false;

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
        {
            var thisExtension = this.Extension as OptionsExtension;
            var otherExtension = other.Extension as OptionsExtension;

            return thisExtension?.ServiceProvider == otherExtension?.ServiceProvider;
        }

        public override int GetServiceProviderHashCode()
        {
            var thisExtension = this.Extension as OptionsExtension;

            return thisExtension?.ServiceProvider?.GetHashCode() ?? 0;
        }

        public override string LogFragment => "using KumaraCommon ";

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) { }
    }
}
