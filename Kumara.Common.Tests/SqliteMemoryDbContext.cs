// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Kumara.Common.Tests;

public class SqliteMemoryDbContext : DbContext
{
    public SqliteMemoryDbContext()
        : base(Options)
    {
        Database.OpenConnection();
        Database.EnsureCreated();
    }

    public override async ValueTask DisposeAsync()
    {
        await Database.CloseConnectionAsync();
        await base.DisposeAsync();
    }

    public static readonly DbContextOptions Options =
        new DbContextOptionsBuilder<SqliteMemoryDbContext>()
            .UseSqlite("Filename=:memory:")
            .UseKumaraCommon()
            .Options;
}
