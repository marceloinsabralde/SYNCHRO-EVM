// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using Kumara.Models;

using Microsoft.EntityFrameworkCore;

namespace Kumara.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Company> Companies { get; set; }
}
