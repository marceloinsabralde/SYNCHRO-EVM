// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Kumara.Common.Database;
using Kumara.Common.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Kumara.Common.Tests.Database;

using SaveTask = Func<DbContext, Task<int>>;

public class ValidateChangesInterceptorTests : IDisposable
{
    readonly TestDbContext dbContext = new();

    public void Dispose()
    {
        dbContext.Dispose();
    }

    [Theory]
    [MemberData(nameof(SaveMethods))]
    public async Task SavingThrowsWhenInvalid(SaveTask saveTask)
    {
        var entity = new TestEntity { Count = -1 };
        dbContext.Entities.Add(entity);

        var ex = await Should.ThrowAsync<ValidationException>(() => saveTask(dbContext));
        ex.Message.ShouldBe("The field Count must be between 0 and 2147483647.");
    }

    [Theory]
    [MemberData(nameof(SaveWithoutValidationMethods))]
    public async Task SavingWithoutValidationSkipsValidation(SaveTask saveTask)
    {
        var entity = new TestEntity { Count = -1 };
        dbContext.Entities.Add(entity);

        var result = await saveTask(dbContext);

        result.ShouldBe(1);
    }

    [Theory]
    [MemberData(nameof(SaveMethods))]
    public async Task SavingShouldNotValidateUnchangedEntities(SaveTask saveTask)
    {
        var entity1 = new TestEntity { Count = -1 };
        dbContext.Entities.Add(entity1);
        var entity2 = new TestEntity { Count = -2 };
        dbContext.Entities.Add(entity2);
        dbContext.SaveChangesWithoutValidation();

        dbContext.Entry(entity2).State = EntityState.Modified;

        var ex = await Should.ThrowAsync<ValidationException>(() => saveTask(dbContext));

        dbContext.Entry(entity2).State = EntityState.Unchanged;

        var result = await saveTask(dbContext);
        result.ShouldBe(0);
    }

    public static List<ITheoryDataRow> SaveMethods =>
        new()
        {
            new TheoryDataRow<SaveTask>(dbContext =>
            {
                var result = dbContext.SaveChanges();
                return Task.FromResult(result);
            })
            {
                Label = "SaveChanges",
            },
            new TheoryDataRow<SaveTask>(dbContext =>
            {
                return dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            })
            {
                Label = "SaveChangesAsync",
            },
        };

    public static List<ITheoryDataRow> SaveWithoutValidationMethods =>
        new()
        {
            new TheoryDataRow<SaveTask>(dbContext =>
            {
                var result = dbContext.SaveChangesWithoutValidation();
                return Task.FromResult(result);
            })
            {
                Label = "SaveChangesWithoutValidation",
            },
            new TheoryDataRow<SaveTask>(dbContext =>
            {
                return dbContext.SaveChangesWithoutValidationAsync(
                    TestContext.Current.CancellationToken
                );
            })
            {
                Label = "SaveChangesWithoutValidationAsync",
            },
        };

    public class TestEntity
    {
        public int Id { get; set; }

        [Range(0, int.MaxValue)]
        public required int Count { get; set; }
    }

    public class TestDbContext() : DbContext(Options)
    {
        public DbSet<TestEntity> Entities { get; set; }

        public static DbContextOptions Options =>
            new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .UseKumaraCommon()
                .Options;
    }
}
