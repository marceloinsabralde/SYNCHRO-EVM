// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using Microsoft.EntityFrameworkCore;

using Kumara.Models;

namespace Kumara.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Company> Companies { get; set; }
}
