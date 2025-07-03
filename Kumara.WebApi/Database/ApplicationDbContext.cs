// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Database;
using Kumara.WebApi.Models;
using Kumara.WebApi.Types;
using Microsoft.EntityFrameworkCore;

namespace Kumara.WebApi.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options),
        ISettingsDbContext<SettingKey>
{
    public DbSet<Company> Companies { get; set; }
    public DbSet<ControlAccount> ControlAccounts { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<UnitOfMeasure> UnitsOfMeasure { get; set; }
    public DbSet<Material> Materials { get; set; }
    public DbSet<ProgressEntry> ProgressEntries { get; set; }
    public DbSet<MaterialActivityAllocation> MaterialActivityAllocations { get; set; }
    public DbSet<ProgressSummary> ProgressSummaries { get; set; }
    public DbSet<Setting<SettingKey>> Settings { get; set; }
    public DbSet<FakeITwin> FakeITwins { get; set; }
}
