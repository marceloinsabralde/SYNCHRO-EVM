// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Database;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Testing;

namespace Kumara.Common.Tests.Database;

public class TimestampedEntityInterceptorTests(
    TimestampedEntityInterceptorTests.TestFixture testFixture
) : IClassFixture<TimestampedEntityInterceptorTests.TestFixture>
{
    private TestDbContext dbContext => testFixture.DbContext;
    private readonly FakeClock fakeClock = testFixture.FakeClock;

    [Fact]
    public void PersistsTimestamps()
    {
        var entity = new TestEntity { Id = Guid.CreateVersion7(), Name = "Foo" };
        entity.CreatedAt.ShouldBe(default);

        dbContext.Entities.Add(entity);
        dbContext.SaveChanges();
        entity.CreatedAt.ShouldNotBe(default);
        entity.UpdatedAt.ShouldNotBe(default);
        entity.UpdatedAt.ShouldBe(entity.CreatedAt);

        fakeClock.AdvanceSeconds(30);

        entity.Name = "New Name";
        dbContext.SaveChanges();
        entity.UpdatedAt.ShouldNotBe(default);
        entity.UpdatedAt.ShouldBe(entity.CreatedAt.Plus(Duration.FromSeconds(30)));

        fakeClock.AdvanceSeconds(30);

        dbContext.SaveChanges();
        entity.UpdatedAt.ShouldNotBe(default);
        entity.UpdatedAt.ShouldBe(entity.CreatedAt.Plus(Duration.FromSeconds(30)));
    }

    [Fact]
    public async Task PersistsTimestampsAsync()
    {
        var entity = new TestEntity { Id = Guid.CreateVersion7(), Name = "Foo" };
        entity.CreatedAt.ShouldBe(default);

        await dbContext.Entities.AddAsync(entity, TestContext.Current.CancellationToken);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        entity.CreatedAt.ShouldNotBe(default);
        entity.UpdatedAt.ShouldNotBe(default);
        entity.UpdatedAt.ShouldBe(entity.CreatedAt);

        fakeClock.AdvanceSeconds(30);

        entity.Name = "New Name";
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        entity.UpdatedAt.ShouldNotBe(default);
        entity.UpdatedAt.ShouldBe(entity.CreatedAt.Plus(Duration.FromSeconds(30)));

        fakeClock.AdvanceSeconds(30);

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        entity.UpdatedAt.ShouldNotBe(default);
        entity.UpdatedAt.ShouldBe(entity.CreatedAt.Plus(Duration.FromSeconds(30)));
    }

    public class TestEntity : ITimestampedEntity
    {
        public required Guid Id { get; init; }
        public required string Name { get; set; }
        public Instant CreatedAt { get; set; }
        public Instant UpdatedAt { get; set; }
    }

    public class TestFixture
    {
        public readonly FakeClock FakeClock;
        public readonly TestDbContext DbContext;

        public TestFixture()
        {
            FakeClock = new FakeClock(Instant.FromUtc(2025, 05, 05, 13, 37));
            var timestampedEntityInterceptor = new TimestampedEntityInterceptor(FakeClock);
            DbContext = new TestDbContext(timestampedEntityInterceptor);
        }
    }

    public class TestDbContext(TimestampedEntityInterceptor timestampedEntityInterceptor)
        : DbContext(Options)
    {
        public DbSet<TestEntity> Entities { get; set; }

        private static readonly DbContextOptions<TestDbContext> Options =
            new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(nameof(TimestampedEntityInterceptorTests))
                .UseSnakeCaseNamingConvention()
                .Options;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(timestampedEntityInterceptor);
            base.OnConfiguring(optionsBuilder);
        }
    }
}
