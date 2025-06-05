// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using Kumara.Models;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Kumara.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IClock clock)
    : DbContext(options)
{
    public DbSet<Company> Companies { get; set; }
    public DbSet<ControlAccount> ControlAccounts { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<UnitOfMeasure> UnitsOfMeasure { get; set; }
    public DbSet<Material> Materials { get; set; }
    public DbSet<ProgressEntry> ProgressEntries { get; set; }
    public DbSet<MaterialActivityAllocation> MaterialActivityAllocations { get; set; }
    public DbSet<ProgressSummary> ProgressSummaries { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Add(_ => new ITwinIdIndexConvention());
    }
}
