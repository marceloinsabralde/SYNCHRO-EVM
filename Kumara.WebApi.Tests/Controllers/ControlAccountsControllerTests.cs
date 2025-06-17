// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;
using System.Net.Http.Json;
using Kumara.Common.Controllers.Responses;
using Kumara.WebApi.Controllers.Responses;

namespace Kumara.WebApi.Tests.Controllers;

public sealed class ControlAccountsControllerTests : DatabaseTestBase
{
    [Fact]
    public async Task Index_Success()
    {
        var iTwinId = Guid.CreateVersion7();
        var otherITwinId = Guid.CreateVersion7();

        // We're supplying a timestamp when we generate our UUIDs so we can control order
        var timestamp = DateTimeOffset.UtcNow.AddDays(-7);

        var controlAccount1 = Factories.ControlAccount(
            id: Guid.CreateVersion7(timestamp.AddDays(0)),
            iTwinId: iTwinId
        );
        var controlAccount2 = Factories.ControlAccount(
            id: Guid.CreateVersion7(timestamp.AddDays(1)),
            iTwinId: iTwinId
        );
        var otherITwinControlAccount = Factories.ControlAccount(
            id: Guid.CreateVersion7(timestamp.AddDays(2)),
            iTwinId: otherITwinId
        );
        await _dbContext.ControlAccounts.AddRangeAsync(
            [controlAccount1, controlAccount2, otherITwinControlAccount],
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = await _client.GetAsync(
            GetPathByName("ListControlAccounts", new { iTwinId }),
            TestContext.Current.CancellationToken
        );

        var apiResponse = await response.ShouldBeApiResponse<
            ListResponse<ControlAccountResponse>
        >();
        var controlAccounts = apiResponse?.Items.ToList();

        controlAccounts.ShouldNotBeNull();
        controlAccounts.ShouldAllBe(controlAccount => controlAccount.ITwinId == iTwinId);
        controlAccounts.Count().ShouldBe(2);
        controlAccounts.ShouldBeEquivalentTo(
            new List<ControlAccountResponse>
            {
                ControlAccountResponse.FromControlAccount(controlAccount1),
                ControlAccountResponse.FromControlAccount(controlAccount2),
            }
        );
    }

    [Fact]
    public async Task Index_WhenITwinIdMissing_BadRequest()
    {
        var response = await _client.GetAsync(
            GetPathByName("ListControlAccounts"),
            TestContext.Current.CancellationToken
        );

        await response.ShouldBeApiErrorBadRequest(
            errorsPattern: @"{""iTwinId"":\[""The iTwinId field is required.""\]}"
        );
    }

    [Fact]
    public async Task Index_WhenITwinNotFound_NotFound()
    {
        var iTwinId = Guid.CreateVersion7();
        var response = await _client.GetAsync(
            GetPathByName("ListControlAccounts", new { iTwinId }),
            TestContext.Current.CancellationToken
        );

        await response.ShouldBeApiErrorNotFound();
    }

    [Fact]
    public async Task Show_Success()
    {
        var controlAccount = Factories.ControlAccount();
        await _dbContext.ControlAccounts.AddAsync(
            controlAccount,
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = await _client.GetAsync(
            GetPathByName("GetControlAccount", new { controlAccount.Id }),
            TestContext.Current.CancellationToken
        );

        var apiResponse = await response.ShouldBeApiResponse<
            ShowResponse<ControlAccountResponse>
        >();
        var controlAccountResponse = apiResponse?.Item;
        controlAccountResponse.ShouldBeEquivalentTo(
            ControlAccountResponse.FromControlAccount(controlAccount)
        );
    }

    [Fact]
    public async Task Show_WhenActivityNotFound_NotFound()
    {
        var response = await _client.GetAsync(
            GetPathByName("GetControlAccount", new { Id = Guid.NewGuid() }),
            TestContext.Current.CancellationToken
        );

        await response.ShouldBeApiErrorNotFound();
    }
}
