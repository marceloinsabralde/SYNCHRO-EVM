// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Database;
using Microsoft.EntityFrameworkCore;

namespace Kumara.WebApi.Tests.Database;

public sealed class ITwinIdIndexConventionTests(ITwinIdIndexConventionTests.TestDbContext dbContext)
    : IClassFixture<ITwinIdIndexConventionTests.TestDbContext>
{
    private readonly TestDbContext _dbContext = dbContext;

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
        var entityType = _dbContext.Model.FindEntityType(typeof(TestEntity))!;
        var indexes = entityType.GetIndexes();
        var hasIndex = indexes.Any(index =>
            index.Properties.Select(p => p.Name).SequenceEqual([propertyName])
        );

        Assert.Equal(shouldHaveIndex, hasIndex);
    }

    public sealed class TestDbContext() : DbContext(Options)
    {
        public DbSet<TestEntity> TestEntity { get; set; }

        public static readonly DbContextOptions<TestDbContext> Options =
            new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(ITwinIdIndexConventionTests))
                .UseSnakeCaseNamingConvention()
                .Options;

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Conventions.Add(_ => new ITwinIdIndexConvention());
        }
    }
}
