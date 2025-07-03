// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel;
using System.Reflection;
using Kumara.Common.Providers;
using Kumara.WebApi.Database;
using Kumara.WebApi.Types;
using Microsoft.EntityFrameworkCore;

namespace Kumara.WebApi.Repositories;

public class SettingsRepository<TRecord, TKey>(
    ISettingsDbContext<TKey> dbContext,
    IITwinPathProvider pathProvider
)
    where TRecord : class
    where TKey : Enum
{
    public async Task<TRecord> FindAsync(Guid iTwinId)
    {
        var iTwinPathIds = (await pathProvider.GetPathFromRootAsync(iTwinId)).ToList();

        var pathSettings = await dbContext
            .Settings.Where(setting => iTwinPathIds.Contains(setting.ITwinId))
            .OrderBy(setting => iTwinPathIds.IndexOf(setting.ITwinId))
            .ToListAsync();

        var result = GetDefaults();

        foreach (var setting in pathSettings)
        {
            PropertyInfo property = result.GetType().GetProperty(setting.Key.ToString())!;
            property.SetValue(result, setting.Value);
        }

        return result;
    }

    public TRecord GetDefaults()
    {
        var result = Activator.CreateInstance<TRecord>();

        foreach (PropertyInfo property in result.GetType().GetProperties())
        {
            foreach (var attr in property.GetCustomAttributes<DefaultValueAttribute>())
            {
                object defaultValue = attr.Value!;
                property.SetValue(result, defaultValue);
            }
        }

        return result;
    }
}
