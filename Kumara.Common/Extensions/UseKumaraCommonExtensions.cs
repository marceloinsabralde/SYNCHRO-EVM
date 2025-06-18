// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Kumara.Common.Extensions
{
    public static class UseKumaraCommonExtensions
    {
        public static DbContextOptionsBuilder UseKumaraCommon(
            this DbContextOptionsBuilder optionsBuilder,
            IServiceProvider? serviceProvider = null
        )
        {
            optionsBuilder.UseSnakeCaseNamingConvention();

            var extension =
                optionsBuilder.Options.FindExtension<OptionsExtension>()
                ?? new OptionsExtension(serviceProvider);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(
                extension
            );

            return optionsBuilder;
        }
    }
}
