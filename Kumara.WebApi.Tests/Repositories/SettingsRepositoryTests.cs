// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Providers;
using Kumara.TestCommon.Helpers;
using Kumara.WebApi.Models;
using Kumara.WebApi.Repositories;
using Kumara.WebApi.Types;

namespace Kumara.WebApi.Tests.Repositories;

public class SettingsRepositoryTests : DatabaseTestBase
{
    private IITwinPathProvider pathProvider = null!;
    private SettingsRepository<Settings, SettingKey> settingsRepository = null!;

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        pathProvider = Substitute.For<IITwinPathProvider>();
        settingsRepository = new(_dbContext, pathProvider);
    }

    [Fact]
    public async Task ReturnsSettingWhenSetDirectly()
    {
        Guid iTwinId1 = Guid.CreateVersion7();
        Guid iTwinId2 = Guid.CreateVersion7();
        pathProvider.GetPathFromRootAsync(iTwinId1).Returns([iTwinId1]);
        pathProvider.GetPathFromRootAsync(iTwinId2).Returns([iTwinId2]);

        _dbContext.Settings.Add(
            new()
            {
                ITwinId = iTwinId1,
                Key = SettingKey.ActualsHaveTime,
                Value = true,
            }
        );
        _dbContext.SaveChanges();

        Settings settings;

        settings = await settingsRepository.FindAsync(iTwinId1);
        settings.ShouldBe(new() { ActualsHaveTime = true });

        settings = await settingsRepository.FindAsync(iTwinId2);
        settings.ShouldBe(new() { ActualsHaveTime = false });
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

        _dbContext.Settings.Add(
            new()
            {
                ITwinId = iTwinId1,
                Key = SettingKey.ActualsHaveTime,
                Value = true,
            }
        );
        _dbContext.Settings.Add(
            new()
            {
                ITwinId = iTwinId3,
                Key = SettingKey.ActualsHaveTime,
                Value = false,
            }
        );
        _dbContext.SaveChanges();

        Settings settings;

        settings = await settingsRepository.FindAsync(iTwinId1);
        settings.ShouldBe(new() { ActualsHaveTime = true });

        settings = await settingsRepository.FindAsync(iTwinId2);
        settings.ShouldBe(new() { ActualsHaveTime = true });

        settings = await settingsRepository.FindAsync(iTwinId3);
        settings.ShouldBe(new() { ActualsHaveTime = false });

        settings = await settingsRepository.FindAsync(iTwinId4);
        settings.ShouldBe(new() { ActualsHaveTime = false });
    }
}
