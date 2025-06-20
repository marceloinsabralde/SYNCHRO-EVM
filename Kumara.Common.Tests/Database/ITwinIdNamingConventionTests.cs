// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Kumara.Common.Tests.Database;

public class ITwinIdNamingConventionTests(ITwinIdNamingConventionTests.TestDbContext dbContext)
    : IClassFixture<ITwinIdNamingConventionTests.TestDbContext>
{
    public sealed class TestEntity
    {
        public required Guid Id { get; set; }
        public required Guid ITwinId { get; set; }
        public required Guid FooId { get; set; }
        public required Guid FooITwinId { get; set; }
        public required Guid[] FooITwinIds { get; set; }
        public required Guid AntiTwinId { get; set; }
    }

    [Theory]
    [InlineData("Id", "id")]
    [InlineData("ITwinId", "itwin_id")]
    [InlineData("FooId", "foo_id")]
    [InlineData("FooITwinId", "foo_itwin_id")]
    [InlineData("FooITwinIds", "foo_itwin_ids")]
    [InlineData("AntiTwinId", "anti_twin_id")]
    public void FixesITwinColumnNames(String propertyName, String expectedColumnName)
    {
        var entityType = dbContext.Model.FindEntityType(typeof(TestEntity))!;
        var propertyType = entityType.GetProperty(propertyName);

        Assert.Equal(expectedColumnName, propertyType.GetColumnName());
    }

    public sealed class TestDbContext() : DbContext(Options)
    {
        public DbSet<TestEntity> TestEntity { get; set; }

        public static readonly DbContextOptions Options =
            new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(ITwinIdNamingConventionTests))
                .UseKumaraCommon()
                .Options;
    }
}
