// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace Kumara.WebApi.Database;

public interface ISettingsDbContext<TRecord, TKey>
    where TRecord : class
    where TKey : Enum
{
    public DbSet<Setting<TRecord, TKey>> Settings { get; set; }
}
