// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.Net;
using System.Net.Http.Json;
using Kumara.WebApi.Controllers.Responses;

namespace Kumara.WebApi.Tests.Controllers;

public sealed class ActivitiesControllerTests : ControllerTestsBase
{
    [Fact]
    public async Task Index_Success()
    {
        var iTwinId = Guid.CreateVersion7();
        var otherITwinId = Guid.CreateVersion7();

        // We're supplying a timestamp when we generate our UUIDs so we can control order
        var timestamp = DateTimeOffset.UtcNow.AddDays(-7);

        var activity1 = Factories.Activity(
            id: Guid.CreateVersion7(timestamp.AddDays(0)),
            iTwinId: iTwinId
        );
        var activity2 = Factories.Activity(
            id: Guid.CreateVersion7(timestamp.AddDays(1)),
            iTwinId: iTwinId
        );
        var otherITwinActivity = Factories.Activity(
            id: Guid.CreateVersion7(timestamp.AddDays(2)),
            iTwinId: otherITwinId
        );
        await _dbContext.Activities.AddRangeAsync(
            [activity1, activity2, otherITwinActivity],
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = await _client.GetAsync(
            $"/api/v1/activities?iTwinId={iTwinId}",
            TestContext.Current.CancellationToken
        );
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var apiResponse = await response.Content.ReadFromJsonAsync<ListResponse<ActivityResponse>>(
            TestContext.Current.CancellationToken
        );
        var activities = apiResponse?.items.ToList();

        activities.ShouldNotBeNull();
        activities.ShouldAllBe(activity => activity.ITwinId == iTwinId);
        activities.Count().ShouldBe(2);
        activities.ShouldBeEquivalentTo(
            new List<ActivityResponse>
            {
                ActivityResponse.FromActivity(activity1),
                ActivityResponse.FromActivity(activity2),
            }
        );
    }

    [Fact]
    public async Task Index_WhenITwinIdMissing_BadRequest()
    {
        var response = await _client.GetAsync(
            "/api/v1/activities",
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
            $"/api/v1/activities?iTwinId={iTwinId}",
            TestContext.Current.CancellationToken
        );

        await response.ShouldBeApiErrorNotFound();
    }

    [Fact]
    public async Task Show_Success()
    {
        var expected = Factories.Activity();
        await _dbContext.Activities.AddAsync(expected, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = await _client.GetAsync(
            $"/api/v1/activities/{expected.Id}",
            TestContext.Current.CancellationToken
        );
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var activity = await response.Content.ReadFromJsonAsync<ActivityResponse>(
            TestContext.Current.CancellationToken
        );
        activity.ShouldBeEquivalentTo(ActivityResponse.FromActivity(expected));
    }

    [Fact]
    public async Task Show_WhenActivityNotFound_NotFound()
    {
        var response = await _client.GetAsync(
            $"/api/v1/activities/{Guid.NewGuid()}",
            TestContext.Current.CancellationToken
        );

        await response.ShouldBeApiErrorNotFound();
    }
}
