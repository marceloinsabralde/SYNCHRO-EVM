// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel;
using Kumara.Common.Extensions;
using Kumara.Common.Providers;
using Kumara.TestCommon.Converters;
using Kumara.WebApi.Database;
using Kumara.WebApi.Models;
using Kumara.WebApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Kumara.WebApi.Tests.Repositories;

public class SettingsRepositoryTests : IDisposable
{
    private readonly TestDbContext dbContext = new();
    private readonly IITwinPathProvider pathProvider;
    private readonly SettingsRepository<TestSettings, TestKey> settingsRepository;

    public SettingsRepositoryTests()
    {
        dbContext = new TestDbContext();
        pathProvider = Substitute.For<IITwinPathProvider>();
        settingsRepository = new(dbContext, pathProvider);
    }

    public void Dispose()
    {
        dbContext.Dispose();
    }

    [Fact]
    public async Task ReturnsSettingWhenSetDirectly()
    {
        Guid iTwinId1 = Guid.CreateVersion7();
        Guid iTwinId2 = Guid.CreateVersion7();
        pathProvider.GetPathFromRootAsync(iTwinId1).Returns([iTwinId1]);
        pathProvider.GetPathFromRootAsync(iTwinId2).Returns([iTwinId2]);

        dbContext.Settings.Add(
            new()
            {
                ITwinId = iTwinId1,
                Key = TestKey.TestBoolean,
                Value = true,
            }
        );
        dbContext.SaveChanges();

        TestSettings settings;

        settings = await settingsRepository.FindAsync(iTwinId1);
        settings.ShouldBe(new() { TestBoolean = true, TestDefaultBoolean = true });

        settings = await settingsRepository.FindAsync(iTwinId2);
        settings.ShouldBe(new() { TestBoolean = false, TestDefaultBoolean = true });
    }

    [Fact]
    public async Task RespectsPathOrderWhenResolving()
    {
        Guid iTwinId1 = Guid.CreateVersion7();
        Guid iTwinId2 = Guid.CreateVersion7();
        Guid iTwinId3 = Guid.CreateVersion7();
        Guid iTwinId4 = Guid.CreateVersion7();
        Guid iTwinId5 = Guid.CreateVersion7();
        pathProvider.GetPathFromRootAsync(iTwinId1).Returns([iTwinId1]);
        pathProvider.GetPathFromRootAsync(iTwinId2).Returns([iTwinId1, iTwinId2]);
        pathProvider.GetPathFromRootAsync(iTwinId3).Returns([iTwinId1, iTwinId2, iTwinId3]);
        pathProvider
            .GetPathFromRootAsync(iTwinId4)
            .Returns([iTwinId1, iTwinId2, iTwinId3, iTwinId4]);
        pathProvider
            .GetPathFromRootAsync(iTwinId5)
            .Returns([iTwinId1, iTwinId2, iTwinId3, iTwinId4, iTwinId5]);

        dbContext.Settings.Add(
            new()
            {
                ITwinId = iTwinId1,
                Key = TestKey.TestBoolean,
                Value = true,
            }
        );
        dbContext.Settings.Add(
            new()
            {
                ITwinId = iTwinId3,
                Key = TestKey.TestBoolean,
                Value = false,
            }
        );
        dbContext.Settings.Add(
            new()
            {
                ITwinId = iTwinId4,
                Key = TestKey.TestDefaultBoolean,
                Value = false,
            }
        );
        dbContext.SaveChanges();

        TestSettings settings;

        settings = await settingsRepository.FindAsync(iTwinId1);
        settings.ShouldBe(new() { TestBoolean = true, TestDefaultBoolean = true });

        settings = await settingsRepository.FindAsync(iTwinId2);
        settings.ShouldBe(new() { TestBoolean = true, TestDefaultBoolean = true });

        settings = await settingsRepository.FindAsync(iTwinId3);
        settings.ShouldBe(new() { TestBoolean = false, TestDefaultBoolean = true });

        settings = await settingsRepository.FindAsync(iTwinId4);
        settings.ShouldBe(new() { TestBoolean = false, TestDefaultBoolean = false });

        settings = await settingsRepository.FindAsync(iTwinId5);
        settings.ShouldBe(new() { TestBoolean = false, TestDefaultBoolean = false });
    }

    [Fact]
    public void CanGetDefaultSettings()
    {
        var settings = settingsRepository.GetDefaults();
        settings.ShouldBe(new() { TestBoolean = false, TestDefaultBoolean = true });
    }

    record TestSettings
    {
        public required bool TestBoolean { get; set; }

        [DefaultValue(true)]
        public required bool TestDefaultBoolean { get; set; }
    }

    enum TestKey
    {
        TestBoolean,
        TestDefaultBoolean,
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
