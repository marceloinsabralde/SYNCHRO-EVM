// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Controllers.Responses;
using Kumara.TestCommon.Extensions;
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
            iTwinId: iTwinId,
            percentComplete: 15.23m
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
        var controlAccounts = apiResponse.Items.ToList();

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
    public async ValueTask Index_PaginationTest()
    {
        var iTwinId = Guid.CreateVersion7();

        var controlAccounts = Enumerable
            .Range(0, 15)
            .Select(index =>
            {
                var timestamp = DateTimeOffset.UtcNow.AddDays(-index);
                return Factories.ControlAccount(
                    id: Guid.CreateVersion7(timestamp),
                    iTwinId: iTwinId
                );
            })
            .OrderBy(controlAccount => controlAccount.Id)
            .ToList();

        var otherITwinControlAccount = Factories.ControlAccount();

        await _dbContext.ControlAccounts.AddRangeAsync(
            controlAccounts.Concat([otherITwinControlAccount]),
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var requestPath = GetPathByName("ListControlAccounts", new { iTwinId, _top = 5 });
        var response = await _client.GetAsync(requestPath, TestContext.Current.CancellationToken);
        var apiResponse = await response.ShouldBeApiResponse<
            PaginatedListResponse<ControlAccountResponse>
        >();
        apiResponse.Links.ShouldHaveLinks(self: requestPath, shouldHaveNext: true);
        var controlAccountsFromResponse = apiResponse.Items.ToList();

        controlAccountsFromResponse.ShouldNotBeNull();
        controlAccountsFromResponse.ShouldAllBe(controlAccount =>
            controlAccount.ITwinId == iTwinId
        );
        var expectedControlAccounts = controlAccounts
            .GetRange(0, 5)
            .Select(ControlAccountResponse.FromControlAccount)
            .ToList();
        controlAccountsFromResponse.ShouldBeEquivalentTo(expectedControlAccounts);

        requestPath = apiResponse.Links.Next!.Href;
        response = await _client.GetAsync(requestPath, TestContext.Current.CancellationToken);
        apiResponse = await response.ShouldBeApiResponse<
            PaginatedListResponse<ControlAccountResponse>
        >();
        apiResponse.Links.ShouldHaveLinks(self: requestPath, shouldHaveNext: true);
        controlAccountsFromResponse = apiResponse.Items.ToList();

        controlAccountsFromResponse.ShouldNotBeNull();
        controlAccountsFromResponse.ShouldAllBe(controlAccount =>
            controlAccount.ITwinId == iTwinId
        );
        expectedControlAccounts = controlAccounts
            .GetRange(5, 5)
            .Select(ControlAccountResponse.FromControlAccount)
            .ToList();
        controlAccountsFromResponse.ShouldBeEquivalentTo(expectedControlAccounts);

        requestPath = apiResponse.Links.Next!.Href;
        response = await _client.GetAsync(requestPath, TestContext.Current.CancellationToken);
        apiResponse = await response.ShouldBeApiResponse<
            PaginatedListResponse<ControlAccountResponse>
        >();
        apiResponse.Links.ShouldHaveLinks(self: requestPath, shouldHaveNext: false);
        controlAccountsFromResponse = apiResponse.Items.ToList();

        controlAccountsFromResponse.ShouldNotBeNull();
        controlAccountsFromResponse.ShouldAllBe(controlAccount =>
            controlAccount.ITwinId == iTwinId
        );
        expectedControlAccounts = controlAccounts
            .GetRange(10, 5)
            .Select(ControlAccountResponse.FromControlAccount)
            .ToList();
        controlAccountsFromResponse.ShouldBeEquivalentTo(expectedControlAccounts);
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
        var controlAccountResponse = apiResponse.Item;
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
