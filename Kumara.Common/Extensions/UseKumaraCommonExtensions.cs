// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using System.Text.Json.Serialization;
using Kumara.Common.Database;
using Kumara.Common.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Kumara.Common.Extensions;

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

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }

    public static void UseKumaraCommon(this JsonOptions options)
    {
        options.JsonSerializerOptions.TypeInfoResolverChain.Insert(
            0,
            new JsonTypeInfoResolverAttributeResolver()
        );

        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower, false)
        );
    }

    public static void UseKumaraCommon(this SwaggerGenOptions options)
    {
        options.UseAllOfToExtendReferenceSchemas();
        options.EnableAnnotations();
    }

    public static void UseKumaraCommon(this OpenApiOptions options)
    {
        options.AddSchemaTransformer<OpenApiSchemaTransformerAttributeTransformer>();
    }
}
