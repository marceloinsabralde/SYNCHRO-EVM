// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using Microsoft.EntityFrameworkCore;

using SYNCHROPerformNextGen.Models;

namespace SYNCHROPerformNextGen.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Company> Companies { get; set; }
}
