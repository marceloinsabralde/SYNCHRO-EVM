// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Kumara.Common.Database;
using Kumara.Common.Extensions;
using Kumara.TestCommon.Converters;
using Kumara.WebApi.Database;
using Kumara.WebApi.Models;
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

    [Theory]
    [MemberData(nameof(IncorrectValueTypeData))]
    public void FailsValidationSavingIncorrectValueType(
        TestKey key,
        Type expectedType,
        object testValue
    )
    {
        dbContext.Settings.Add(
            new()
            {
                ITwinId = Guid.CreateVersion7(),
                Key = key,
                Value = testValue,
            }
        );

        var ex = Should.Throw<ValidationException>(() =>
        {
            dbContext.SaveChanges();
        });
        ex.Message.ShouldBe($"{key} values must be convertable to {expectedType}");
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
    public void FailsValidationSavingInvalidKey()
    {
        dbContext.Settings.Add(
            new()
            {
                ITwinId = Guid.CreateVersion7(),
                Key = TestKey.TestInvalid,
                Value = false,
            }
        );

        var ex = Should.Throw<ValidationException>(() =>
        {
            dbContext.SaveChanges();
        });
        ex.Message.ShouldBe($"Could not find {TestKey.TestInvalid} in {typeof(TestSettings)}");
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

    public static TheoryData<TestKey, Type, object> IncorrectValueTypeData = new()
    {
        { TestKey.TestBoolean, typeof(bool), 42 },
        { TestKey.TestBoolean, typeof(bool), 12345678901234567890m },
        { TestKey.TestBoolean, typeof(bool), "test" },
        { TestKey.TestDecimal, typeof(decimal), true },
        { TestKey.TestDecimal, typeof(decimal), "test" },
        { TestKey.TestString, typeof(string), false },
        { TestKey.TestString, typeof(string), 42 },
        { TestKey.TestString, typeof(string), 12345678901234567890m },
        { TestKey.TestInteger, typeof(int), false },
        { TestKey.TestInteger, typeof(int), 42 },
        { TestKey.TestInteger, typeof(int), 12345678901234567890m },
        { TestKey.TestInteger, typeof(int), "test" },
    };

    record TestSettings
    {
        // valid
        public required bool TestBoolean { get; set; }
        public required decimal TestDecimal { get; set; }
        public required string TestString { get; set; }

        // unsupported types
        public required int TestInteger { get; set; }
    }

    public enum TestKey
    {
        TestBoolean,
        TestDecimal,
        TestString,
        TestInteger,
        TestInvalid,
    }

    class TestDbContext() : DbContext(Options), ISettingsDbContext<TestSettings, TestKey>
    {
        public DbSet<Setting<TestSettings, TestKey>> Settings { get; set; }

        public static DbContextOptions Options =>
            new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .UseKumaraCommon()
                .Options;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Setting<TestSettings, TestKey>>()
                .Property(e => e.Value)
                .HasConversion(new ObjectJsonStringValueConverter());
        }
    }
}
