// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Extensions;
using Kumara.Common.Providers;
using Kumara.TestCommon.Converters;
using Kumara.TestCommon.Helpers;
using Kumara.WebApi.Database;
using Kumara.WebApi.Models;
using Kumara.WebApi.Repositories;
using Kumara.WebApi.Types;
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
        settings.ShouldBe(new() { TestBoolean = true });

        settings = await settingsRepository.FindAsync(iTwinId2);
        settings.ShouldBe(new() { TestBoolean = false });
    }

    [Fact]
    public async Task RespectsPathOrderWhenResolving()
    {
        Guid iTwinId1 = Guid.CreateVersion7();
        Guid iTwinId2 = Guid.CreateVersion7();
        Guid iTwinId3 = Guid.CreateVersion7();
        Guid iTwinId4 = Guid.CreateVersion7();
        pathProvider.GetPathFromRootAsync(iTwinId1).Returns([iTwinId1]);
        pathProvider.GetPathFromRootAsync(iTwinId2).Returns([iTwinId1, iTwinId2]);
        pathProvider.GetPathFromRootAsync(iTwinId3).Returns([iTwinId1, iTwinId2, iTwinId3]);
        pathProvider
            .GetPathFromRootAsync(iTwinId4)
            .Returns([iTwinId1, iTwinId2, iTwinId3, iTwinId4]);

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
        dbContext.SaveChanges();

        TestSettings settings;

        settings = await settingsRepository.FindAsync(iTwinId1);
        settings.ShouldBe(new() { TestBoolean = true });

        settings = await settingsRepository.FindAsync(iTwinId2);
        settings.ShouldBe(new() { TestBoolean = true });

        settings = await settingsRepository.FindAsync(iTwinId3);
        settings.ShouldBe(new() { TestBoolean = false });

        settings = await settingsRepository.FindAsync(iTwinId4);
        settings.ShouldBe(new() { TestBoolean = false });
    }

    record TestSettings
    {
        public required bool TestBoolean { get; set; }
    }

    enum TestKey
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
