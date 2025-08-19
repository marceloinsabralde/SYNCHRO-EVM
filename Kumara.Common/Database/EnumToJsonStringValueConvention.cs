// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Converters;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kumara.Common.Database;

public class EnumToJsonStringValueConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context
    )
    {
        var properties = modelBuilder
            .Metadata.GetEntityTypes()
            .SelectMany(entityType =>
                entityType.GetDeclaredProperties().Where(property => property.ClrType.IsEnum)
            );

        foreach (var property in properties)
        {
            var converterType = typeof(EnumToJsonStringValueConverter<>).MakeGenericType(
                property.ClrType
            );
            var converter = (ValueConverter)Activator.CreateInstance(converterType)!;
            property.SetValueConverter(converter);
        }
    }
}
