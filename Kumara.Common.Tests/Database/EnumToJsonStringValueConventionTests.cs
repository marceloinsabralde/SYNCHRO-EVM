// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.TestCommon.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Kumara.Common.Tests.Database;

public class EnumToJsonStringValueConventionTests(
    EnumToJsonStringValueConventionTests.TestDbContext dbContext
) : IClassFixture<EnumToJsonStringValueConventionTests.TestDbContext>
{
    [Fact]
    public void StoresEnumsAsJsonStringValues()
    {
        var entity = new TestEntity { Id = Guid.CreateVersion7(), TestEnum = TestEnum.FirstValue };
        dbContext.Entities.Add(entity);
        dbContext.SaveChanges();

        var rowData = DatabaseHelpers.GetEntityRowData(dbContext, entity);
        rowData.ShouldContain(new KeyValuePair<string, object?>("test_enum", "first_value"));

        rowData["test_enum"] = "second_value";
        DatabaseHelpers.SetEntityRowData(dbContext, entity, rowData);
        dbContext.Entry(entity).Reload();

        entity.TestEnum.ShouldBe(TestEnum.SecondValue);
    }

    public enum TestEnum
    {
        FirstValue,
        SecondValue,
    }

    public class TestEntity
    {
        public required Guid Id { get; init; }
        public required TestEnum TestEnum { get; set; }
    }

    public class TestDbContext : SqliteMemoryDbContext
    {
        public DbSet<TestEntity> Entities { get; set; }
    }
}
