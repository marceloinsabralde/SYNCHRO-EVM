// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.Models;
using Microsoft.EntityFrameworkCore;

namespace Kumara.EventSource.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<Event> Events { get; set; }
}
