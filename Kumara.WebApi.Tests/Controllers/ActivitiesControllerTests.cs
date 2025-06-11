// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;
using System.Net.Http.Json;
using Kumara.WebApi.Controllers.Responses;
using Kumara.WebApi.Types;

namespace Kumara.WebApi.Tests.Controllers;

public sealed class ActivitiesControllerTests : DatabaseTestBase
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
        var apiResponse = await response.ShouldBeApiResponse<ListResponse<ActivityResponse>>();
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
    public async ValueTask Index_WithControlAccountFilter()
    {
        var iTwinId = Guid.CreateVersion7();
        var otherITwinId = Guid.CreateVersion7();

        // We're supplying a timestamp when we generate our UUIDs so we can control order
        var timestamp = DateTimeOffset.UtcNow.AddDays(-7);

        var controlAccount = Factories.ControlAccount(iTwinId: iTwinId);
        await _dbContext.ControlAccounts.AddAsync(
            controlAccount,
            TestContext.Current.CancellationToken
        );

        var activity1 = Factories.Activity(
            id: Guid.CreateVersion7(timestamp.AddDays(0)),
            iTwinId: iTwinId,
            controlAccount: controlAccount
        );
        var activity2 = Factories.Activity(
            id: Guid.CreateVersion7(timestamp.AddDays(1)),
            iTwinId: iTwinId,
            controlAccount: controlAccount
        );
        var otherControlAccountActivity = Factories.Activity(
            id: Guid.CreateVersion7(timestamp.AddDays(2)),
            iTwinId: iTwinId
        );
        var otherITwinActivity = Factories.Activity(
            id: Guid.CreateVersion7(timestamp.AddDays(2)),
            iTwinId: otherITwinId
        );
        await _dbContext.Activities.AddRangeAsync(
            [activity1, activity2, otherControlAccountActivity, otherITwinActivity],
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = await _client.GetAsync(
            $"/api/v1/activities?iTwinId={iTwinId}&controlAccountId={controlAccount.Id}",
            TestContext.Current.CancellationToken
        );
        var apiResponse = await response.ShouldBeApiResponse<ListResponse<ActivityResponse>>();
        var activities = apiResponse?.items.ToList();

        activities.ShouldNotBeNull();
        activities.ShouldAllBe(activity => activity.ITwinId == iTwinId);
        activities.ShouldAllBe(activity => activity.ControlAccountId == controlAccount.Id);
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

        var activity = await response.ShouldBeApiResponse<ActivityResponse>();
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

    [Fact]
    public async Task Update_WithAllFields_Accepted()
    {
        var existingActivity = Factories.Activity(
            actualStart: new DateWithOptionalTime
            {
                DateTime = new DateTimeOffset(
                    new DateOnly(year: 2025, month: 1, day: 1),
                    TimeOnly.MinValue,
                    TimeSpan.Zero
                ),
                HasTime = false,
            },
            actualFinish: null
        );
        await _dbContext.Activities.AddAsync(
            existingActivity,
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/activities/{existingActivity.Id}",
            new { actualStart = "2025-03-18T10:47:05.288+10:00", actualFinish = "2025-03-19" },
            TestContext.Current.CancellationToken
        );
        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);

        response = await _client.GetAsync(
            $"/api/v1/activities/{existingActivity.Id}",
            TestContext.Current.CancellationToken
        );

        var activity = await response.ShouldBeApiResponse<ActivityResponse>();
        activity.ShouldNotBeNull();
        activity.ActualStart.ShouldBe(
            new DateWithOptionalTime
            {
                DateTime = new DateTimeOffset(
                    new DateOnly(year: 2025, month: 3, day: 18),
                    new TimeOnly(00, 47, 05, 288),
                    TimeSpan.Zero
                ),
                HasTime = true,
            }
        );
        activity.ActualFinish.ShouldBe(
            new DateWithOptionalTime
            {
                DateTime = new DateTimeOffset(
                    new DateOnly(year: 2025, month: 3, day: 19),
                    TimeOnly.MinValue,
                    TimeSpan.Zero
                ),
                HasTime = false,
            }
        );

        // TODO: Ensure an Event is emitted to update the specified Activity
    }

    [Fact]
    public async Task Update_WithPartialFields_Accepted()
    {
        var existingActivity = Factories.Activity(
            actualStart: new DateWithOptionalTime
            {
                DateTime = new DateTimeOffset(
                    new DateOnly(year: 2025, month: 1, day: 1),
                    TimeOnly.MinValue,
                    TimeSpan.Zero
                ),
                HasTime = false,
            },
            actualFinish: new DateWithOptionalTime
            {
                DateTime = new DateTimeOffset(
                    new DateOnly(year: 2025, month: 2, day: 1),
                    TimeOnly.MinValue,
                    TimeSpan.Zero
                ),
                HasTime = false,
            }
        );
        await _dbContext.Activities.AddAsync(
            existingActivity,
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/activities/{existingActivity.Id}",
            new { actualFinish = "2025-03-19" },
            TestContext.Current.CancellationToken
        );
        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);

        response = await _client.GetAsync(
            $"/api/v1/activities/{existingActivity.Id}",
            TestContext.Current.CancellationToken
        );

        var activity = await response.ShouldBeApiResponse<ActivityResponse>();
        activity.ShouldNotBeNull();
        activity.ActualStart.ShouldBe(existingActivity.ActualStart);
        activity.ActualFinish.ShouldBe(
            new DateWithOptionalTime
            {
                DateTime = new DateTimeOffset(
                    new DateOnly(year: 2025, month: 3, day: 19),
                    TimeOnly.MinValue,
                    TimeSpan.Zero
                ),
                HasTime = false,
            }
        );

        // TODO: Ensure an Event is emitted to update the specified Activity
    }

    [Fact]
    public async Task Update_WithBadData_BadRequest()
    {
        var existingActivity = Factories.Activity(
            actualStart: new DateWithOptionalTime
            {
                DateTime = new DateTimeOffset(
                    new DateOnly(year: 2025, month: 1, day: 1),
                    TimeOnly.MinValue,
                    TimeSpan.Zero
                ),
                HasTime = false,
            }
        );
        await _dbContext.Activities.AddAsync(
            existingActivity,
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/activities/{existingActivity.Id}",
            new { actualStart = "foo" },
            TestContext.Current.CancellationToken
        );

        await response.ShouldBeApiErrorBadRequest(
            errorsPattern: @"""\$\.actualStart"":\[""The JSON value could not be converted to"
        );

        // TODO: Ensure no Event is emitted
    }

    [Fact]
    public async Task Update_WhenActivityNotFound_NotFound()
    {
        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/activities/{Guid.CreateVersion7()}",
            new { },
            TestContext.Current.CancellationToken
        );

        await response.ShouldBeApiErrorNotFound();

        // TODO: Ensure no Event is emitted
    }
}
