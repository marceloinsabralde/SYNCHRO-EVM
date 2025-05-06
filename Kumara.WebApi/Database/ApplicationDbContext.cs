// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using Kumara.Models;
using Microsoft.EntityFrameworkCore;

namespace Kumara.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<Company> Companies { get; set; }
    public DbSet<ControlAccount> ControlAccounts { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<UnitOfMeasure> UnitsOfMeasure { get; set; }
    public DbSet<Material> Materials { get; set; }
    public DbSet<ProgressEntry> ProgressEntries { get; set; }
    public DbSet<MaterialActivityAllocation> MaterialActivityAllocations { get; set; }
}
