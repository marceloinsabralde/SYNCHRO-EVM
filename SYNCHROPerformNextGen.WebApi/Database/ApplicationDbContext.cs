// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using Microsoft.EntityFrameworkCore;

namespace SYNCHROPerformNextGen.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
}
