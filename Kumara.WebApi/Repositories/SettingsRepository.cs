// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Reflection;
using Kumara.Common.Providers;
using Kumara.WebApi.Database;
using Kumara.WebApi.Types;
using Microsoft.EntityFrameworkCore;

namespace Kumara.WebApi.Repositories;

public class SettingsRepository(ApplicationDbContext dbContext, IITwinPathProvider pathProvider)
{
    public async Task<Settings> FindAsync(Guid iTwinId)
    {
        var iTwinPathIds = (await pathProvider.GetPathFromRootAsync(iTwinId)).ToList();

        var pathSettings = await dbContext
            .Settings.Where(setting => iTwinPathIds.Contains(setting.ITwinId))
            .OrderBy(setting => iTwinPathIds.IndexOf(setting.ITwinId))
            .ToListAsync();

        var result = Activator.CreateInstance<Settings>();

        foreach (var setting in pathSettings)
        {
            PropertyInfo property = result.GetType().GetProperty(setting.Key.ToString())!;
            property.SetValue(result, setting.Value);
        }

        return result;
    }
}
