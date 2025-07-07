// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Database;
using Kumara.Common.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Kumara.Common.Tests.Database;

public sealed class ITwinIdIndexConventionTests(ITwinIdIndexConventionTests.TestDbContext dbContext)
    : IClassFixture<ITwinIdIndexConventionTests.TestDbContext>
{
    public sealed class TestEntity
    {
        public required Guid Id { get; set; }
        public required Guid ITwinId { get; set; }
        public required Guid FooId { get; set; }
        public required Guid FooITwinId { get; set; }
    }

    [Theory]
    [InlineData("Id", false)]
    [InlineData("FooBarId", false)]
    [InlineData("FooITwinId", true)]
    [InlineData("ITwinId", true)]
    public void AddsIndexToPropertiesEndingWithITwinId(string propertyName, bool shouldHaveIndex)
    {
        var entityType = dbContext.Model.FindEntityType(typeof(TestEntity))!;
        var indexes = entityType.GetIndexes();
        var hasIndex = indexes.Any(index =>
            index.Properties.Select(p => p.Name).SequenceEqual([propertyName])
        );

        Assert.Equal(shouldHaveIndex, hasIndex);
    }

    public sealed class TestDbContext() : DbContext(Options)
    {
        public DbSet<TestEntity> Entities { get; set; }

        public static readonly DbContextOptions Options =
            new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(ITwinIdIndexConventionTests))
                .UseKumaraCommon()
                .Options;
    }
}
