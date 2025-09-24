// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace Kumara.Common.Extensions;

public static class ConfigureDataProtectionExtensions
{
    public static void ConfigureDataProtection(this WebApplicationBuilder builder)
    {
        var options = builder.RegisterOptionalConfig<Kumara.Common.Options.DataProtectionOptions>();
        if (options is null)
            return;

        builder
            .Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(options.KeyPath));
    }
}
