// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Controllers.Responses;
using Kumara.TestCommon.Extensions;
using Kumara.WebApi.Types;

namespace Kumara.WebApi.Tests.Controllers;

public sealed class SettingsControllerTests : ApplicationTestBase
{
    [Fact]
    public async Task GetSettings_ReturnsSettingsForItwinWithSettings()
    {
        var iTwinId = Guid.CreateVersion7();

        _dbContext.FakeITwins.Add(new() { Id = iTwinId });
        _dbContext.Settings.Add(
            new()
            {
                ITwinId = iTwinId,
                Key = SettingKey.ActualsHaveTime,
                Value = true,
            }
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = await _client.GetAsync(
            GetPathByName("GetSettings", new { iTwinId }),
            TestContext.Current.CancellationToken
        );
        var apiResponse = await response.ShouldBeApiResponse<ShowResponse<Settings>>();
        var settings = apiResponse.Item;

        settings.ShouldBe(new() { ActualsHaveTime = true });
    }

    [Fact]
    public async Task GetSettings_ReturnsDefaultsForValidItwinWithoutSettings()
    {
        var iTwinId = Guid.CreateVersion7();

        _dbContext.FakeITwins.Add(new() { Id = iTwinId });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = await _client.GetAsync(
            GetPathByName("GetSettings", new { iTwinId }),
            TestContext.Current.CancellationToken
        );
        var apiResponse = await response.ShouldBeApiResponse<ShowResponse<Settings>>();
        var settings = apiResponse.Item;

        settings.ShouldBe(new() { ActualsHaveTime = false });
    }

    [Fact]
    public async Task GetSettings_ReturnsNotFoundForInvalidItwin()
    {
        var iTwinId = Guid.CreateVersion7();

        var response = await _client.GetAsync(
            GetPathByName("GetSettings", new { iTwinId }),
            TestContext.Current.CancellationToken
        );

        await response.ShouldBeApiErrorNotFound();
    }
}
