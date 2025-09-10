// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;
using Kumara.WebApi.Models;
using Kumara.WebApi.Providers;

namespace Kumara.WebApi.Tests.Providers;

public class FakeITwinProviderTests : ApplicationTestBase
{
    [Fact]
    public async Task ReturnsSuccessWhenFakeITwinExists()
    {
        var iTwinId = Guid.CreateVersion7();

        var iTwin = new FakeITwin() { Id = iTwinId, DisplayName = "Fake iTwin" };
        _dbContext.FakeITwins.Add(iTwin);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var provider = new FakeITwinProvider(_dbContext);
        var result = await provider.GetAsync(iTwinId);

        result.ShouldNotBeNull();
        result.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.iTwin.ShouldNotBeNull();
        result.iTwin.Id.ShouldBe(iTwin.Id);
        result.iTwin.DisplayName.ShouldBe(iTwin.DisplayName);
        result.Error.ShouldBeNull();
    }

    [Fact]
    public async Task ReturnsNotFoundWhenFakeITwinDoesNotExist()
    {
        var iTwinId = Guid.CreateVersion7();

        var provider = new FakeITwinProvider(_dbContext);
        var result = await provider.GetAsync(iTwinId);

        result.ShouldNotBeNull();
        result.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        result.iTwin.ShouldBeNull();
        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe("iTwinNotFound");
        result.Error.Message.ShouldBe("Requested iTwin is not available.");
    }
}
