// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kumara.WebApi.Tests;

public static class DatabaseHelpers
{
    public static IDictionary<string, object?> GetEntityColumnValues(
        DbContext dbContext,
        object entity
    )
    {
        var entityType = dbContext.Model.FindEntityType(entity.GetType())!;

        return entityType
            .GetProperties()
            .ToDictionary(
                property => property.GetColumnName(),
                property =>
                    property switch
                    {
                        { PropertyInfo: not null } => property.PropertyInfo.GetValue(entity),
                        { FieldInfo: not null } => property.FieldInfo.GetValue(entity),
                        _ => throw new ArgumentException(
                            $"Unknown property type for {property.GetColumnName()}"
                        ),
                    }
            );
    }

    public static void SetEntityColumnValues(
        DbContext dbContext,
        object entity,
        IDictionary<string, object?> values
    )
    {
        var entityType = dbContext.Model.FindEntityType(entity.GetType())!;

        var propertiesByColumnName = entityType
            .GetProperties()
            .ToDictionary(property => property.GetColumnName(), property => property);

        foreach (var item in values)
        {
            var property = propertiesByColumnName[item.Key];

            switch (property)
            {
                case { PropertyInfo: not null }:
                    property.PropertyInfo.SetValue(entity, item.Value);
                    break;
                case { FieldInfo: not null }:
                    property.FieldInfo.SetValue(entity, item.Value);
                    break;
                default:
                    throw new ArgumentException($"Unknown property type for {item.Key}");
            }
        }
    }
}
