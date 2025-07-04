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
    [InlineData(true)]
    [InlineData(false)]
    public void CanRoundTripSupportedValueTypes(object testValue)
    {
        dbContext.Settings.Add(
            new()
            {
                ITwinId = Guid.CreateVersion7(),
                Key = TestKey.TestBoolean,
                Value = testValue,
            }
        );
        dbContext.SaveChanges();

        var setting = dbContext.Settings.First();
        setting.Value.ShouldBe(testValue);
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

    public enum TestKey
    {
        TestBoolean,
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
