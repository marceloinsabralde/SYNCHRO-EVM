// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSourceToo.Models;
using Microsoft.EntityFrameworkCore;

namespace Kumara.EventSourceToo.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<Event> Events { get; set; }
}
