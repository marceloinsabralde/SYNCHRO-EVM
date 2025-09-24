// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Microsoft.EntityFrameworkCore;

namespace Kumara.TestCommon.Helpers;

public static class DatabaseHelpers
{
    public static IDictionary<string, object?> GetEntityRowData(DbContext dbContext, object entity)
    {
        var connection = dbContext.Database.GetDbConnection();
        var entityType = dbContext.Model.FindEntityType(entity.GetType())!;
        var tableName = entityType.GetTableName()!;
        var idValue = entity.GetType().GetProperty("Id")?.GetValue(entity);

        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {tableName} WHERE id = @id";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@id";
        parameter.Value = idValue;
        command.Parameters.Add(parameter);
        using var reader = command.ExecuteReader();

        if (!reader.Read())
            throw new InvalidOperationException("Expected one row, but got none.");

        var result = new Dictionary<string, object?>();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            result[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
        }

        if (reader.Read())
            throw new InvalidOperationException("Expected one row, but got more than one.");

        return result;
    }

    public static void SetEntityRowData(
        DbContext dbContext,
        object entity,
        IDictionary<string, object?> rowData
    )
    {
        var connection = dbContext.Database.GetDbConnection();
        var entityType = dbContext.Model.FindEntityType(entity.GetType())!;
        var tableName = entityType.GetTableName()!;
        var idValue = entity.GetType().GetProperty("Id")?.GetValue(entity);

        using var command = connection.CreateCommand();

        var setClauses = new List<string>();
        foreach (var item in rowData)
        {
            var paramName = $"@{item.Key}";
            setClauses.Add($"{item.Key} = {paramName}");

            var param = command.CreateParameter();
            param.ParameterName = paramName;
            param.Value = item.Value ?? DBNull.Value;
            command.Parameters.Add(param);
        }

        command.CommandText =
            $"UPDATE {tableName} SET {string.Join(", ", setClauses)} WHERE id = @id";

        command.ExecuteNonQuery();
    }
}
