// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Kumara.Common.Database;
using Kumara.Common.Extensions;
using Kumara.TestCommon.Converters;
using Kumara.TestCommon.Extensions;
using Kumara.WebApi.Database;
using Kumara.WebApi.Models;
using Kumara.WebApi.Types;
using Microsoft.EntityFrameworkCore;

namespace Kumara.WebApi.Tests.Models;

public class SettingTests
{
    readonly TestDbContext dbContext = new();

    [Theory]
    [MemberData(nameof(RoundTripData))]
    public void CanRoundTripSupportedValueTypes(TestKey key, Type expectedType, object testValue)
    {
        dbContext.Settings.Add(
            new()
            {
                ITwinId = Guid.CreateVersion7(),
                Key = key,
                Value = testValue,
            }
        );
        dbContext.SaveChanges();

        var setting = dbContext.Settings.First();
        setting.Value.ShouldBe(testValue);
        setting.Value.GetType().ShouldBe(expectedType);
    }

    [Fact]
    public void FailsValidationSavingUnsupportedValueType()
    {
        dbContext.Settings.Add(
            new()
            {
                ITwinId = Guid.CreateVersion7(),
                Key = TestKey.TestBoolean,
                Value = new int[] { },
            }
        );

        var ex = Should.Throw<ValidationException>(() =>
        {
            dbContext.SaveChanges();
        });
        ex.Message.ShouldBe("Array types are not supported");
    }

    [Fact]
    public void ErrorsReadingUnsupportedValueType()
    {
        dbContext.Settings.Add(
            new()
            {
                ITwinId = Guid.CreateVersion7(),
                Key = TestKey.TestBoolean,
                Value = new int[] { },
            }
        );

        dbContext.SaveChangesWithoutValidation();
        var setting = dbContext.Settings.First();

        var ex = Should.Throw<InvalidOperationException>(() =>
        {
            var _ = setting.Value;
        });
        ex.Message.ShouldBe("Array types are not supported");
    }

    public static TheoryData<TestKey, Type, object> RoundTripData = new()
    {
        { TestKey.TestBoolean, typeof(bool), true },
        { TestKey.TestBoolean, typeof(bool), false },
        { TestKey.TestDecimal, typeof(decimal), 42 },
        { TestKey.TestDecimal, typeof(decimal), 12345678901234567890m },
        { TestKey.TestString, typeof(string), "test" },
    };

    public enum TestKey
    {
        TestBoolean,
        TestDecimal,
        TestString,
    }

    class TestDbContext() : DbContext(Options), ISettingsDbContext<TestKey>
    {
        public DbSet<Setting<TestKey>> Settings { get; set; }

        public static DbContextOptions Options =>
            new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .UseKumaraCommon()
                .Options;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Setting<TestKey>>()
                .Property(e => e.Value)
                .HasConversion(new ObjectJsonStringValueConverter());
        }
    }
}
