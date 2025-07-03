// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace Kumara.WebApi.Database;

public interface ISettingsDbContext<TKey>
    where TKey : Enum
{
    public DbSet<Setting<TKey>> Settings { get; set; }
}
