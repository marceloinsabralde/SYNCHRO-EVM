// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Bentley.ConnectCoreLibs.Providers.Abstractions.ConnectedContextModels;
using Bentley.ConnectCoreLibs.Providers.Abstractions.Interfaces;
using Bentley.ConnectCoreLibs.Providers.Abstractions.OData;
using Kumara.Common.Providers;

namespace Kumara.Common.Tests.Providers;

public class ITwinPathProviderTests
{
    private readonly IITwinProvider iTwinProvider;
    private readonly ITwinPathProvider pathProvider;

    public ITwinPathProviderTests()
    {
        iTwinProvider = Substitute.For<IITwinProvider>();
        pathProvider = new(iTwinProvider);
    }

    private static Task<ITwinResponse> MockResponse(Guid id, Guid? parentId, Guid accountId)
    {
        var response = new ITwinResponse
        {
            iTwin = new ITwin
            {
                Id = id,
                ParentId = parentId,
                iTwinAccountId = accountId,
            },
        };

        return Task.FromResult(response);
    }

    private static ODataRequestOptions RequestOptionsArg =>
        Arg.Is<ODataRequestOptions>(opts => opts.Select == "id,parentId,iTwinAccountId");

    [Fact]
    public async Task ReturnsPathForDirectAccountProject()
    {
        var accountId = Guid.NewGuid();
        var iTwinId = Guid.NewGuid();

        iTwinProvider
            .GetAsync(iTwinId, RequestOptionsArg)
            .Returns(MockResponse(iTwinId, accountId, accountId));

        var path = await pathProvider.GetPathFromRootAsync(iTwinId);

        iTwinProvider.ReceivedCalls().Count().ShouldBe(1);
        await iTwinProvider.Received(1).GetAsync(iTwinId, RequestOptionsArg);

        path.ShouldBe([accountId, iTwinId]);
    }

    [Fact]
    public async Task ReturnsPathForChildProject()
    {
        var accountId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        iTwinProvider
            .GetAsync(childId, RequestOptionsArg)
            .Returns(MockResponse(childId, parentId, accountId));
        iTwinProvider
            .GetAsync(parentId, RequestOptionsArg)
            .Returns(MockResponse(parentId, accountId, accountId));

        var path = await pathProvider.GetPathFromRootAsync(childId);

        iTwinProvider.ReceivedCalls().Count().ShouldBe(2);
        await iTwinProvider.Received(1).GetAsync(childId, RequestOptionsArg);
        await iTwinProvider.Received(1).GetAsync(parentId, RequestOptionsArg);

        path.ShouldBe([accountId, parentId, childId]);
    }

    [Fact]
    public async Task ReturnsPathForOrphan()
    {
        var iTwinId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        iTwinProvider
            .GetAsync(iTwinId, RequestOptionsArg)
            .Returns(MockResponse(iTwinId, null, accountId));

        var path = await pathProvider.GetPathFromRootAsync(iTwinId);

        iTwinProvider.ReceivedCalls().Count().ShouldBe(1);
        await iTwinProvider.Received(1).GetAsync(iTwinId, RequestOptionsArg);

        path.ShouldBe([iTwinId]);
    }

    [Fact]
    public async Task ThrowsInvalidOperationForErrorResponse()
    {
        var iTwinId = Guid.NewGuid();

        iTwinProvider
            .GetAsync(iTwinId, RequestOptionsArg)
            .Returns(
                Task.FromResult(
                    new ITwinResponse()
                    {
                        Error = new()
                        {
                            Code = "iTwinNotFound",
                            Message = "Requested iTwin is not available.",
                        },
                    }
                )
            );

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            pathProvider.GetPathFromRootAsync(iTwinId)
        );

        iTwinProvider.ReceivedCalls().Count().ShouldBe(1);
        await iTwinProvider.Received(1).GetAsync(iTwinId, RequestOptionsArg);

        ex.Message.ShouldBe(
            $"Error retrieving iTwin {iTwinId}: iTwinNotFound: Requested iTwin is not available."
        );
    }

    [Fact]
    public async Task ThrowsInvalidOperationForNullResponse()
    {
        var iTwinId = Guid.NewGuid();

        iTwinProvider
            .GetAsync(iTwinId, RequestOptionsArg)
            .Returns(Task.FromResult(new ITwinResponse()));

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            pathProvider.GetPathFromRootAsync(iTwinId)
        );

        iTwinProvider.ReceivedCalls().Count().ShouldBe(1);
        await iTwinProvider.Received(1).GetAsync(iTwinId, RequestOptionsArg);

        ex.Message.ShouldBe($"Error retrieving iTwin {iTwinId}: iTwin is null");
    }

    [Fact]
    public async Task ThrowsInvalidOperationForEmptyResponse()
    {
        var iTwinId = Guid.NewGuid();

        iTwinProvider
            .GetAsync(iTwinId, RequestOptionsArg)
            .Returns(Task.FromResult(new ITwinResponse { iTwin = new() }));

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            pathProvider.GetPathFromRootAsync(iTwinId)
        );

        iTwinProvider.ReceivedCalls().Count().ShouldBe(1);
        await iTwinProvider.Received(1).GetAsync(iTwinId, RequestOptionsArg);

        ex.Message.ShouldBe($"Error retrieving iTwin {iTwinId}: iTwin is null");
    }

    [Fact]
    public async Task ThrowsInvalidOperationForCircularReference()
    {
        var iTwinId1 = Guid.NewGuid();
        var iTwinId2 = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        iTwinProvider
            .GetAsync(iTwinId1, RequestOptionsArg)
            .Returns(MockResponse(iTwinId1, iTwinId2, accountId));
        iTwinProvider
            .GetAsync(iTwinId2, RequestOptionsArg)
            .Returns(MockResponse(iTwinId2, iTwinId1, accountId));

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            pathProvider.GetPathFromRootAsync(iTwinId1)
        );

        iTwinProvider.ReceivedCalls().Count().ShouldBe(2);
        await iTwinProvider.Received(1).GetAsync(iTwinId1, RequestOptionsArg);
        await iTwinProvider.Received(1).GetAsync(iTwinId2, RequestOptionsArg);

        ex.Message.ShouldBe(
            $"Cycle detected in iTwin hierarchy: {iTwinId1} -> {iTwinId2} -> {iTwinId1}"
        );
    }
}
