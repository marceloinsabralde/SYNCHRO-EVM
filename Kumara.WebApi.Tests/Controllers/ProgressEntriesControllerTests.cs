// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;
using Kumara.Common.Controllers.Responses;
using Kumara.TestCommon.Extensions;
using Kumara.WebApi.Controllers.Requests;
using Kumara.WebApi.Models;
using NodaTime;

namespace Kumara.WebApi.Tests.Controllers;

public sealed class ProgressEntriesControllerTests : DatabaseTestBase
{
    async Task<Dictionary<string, object>> BuildCreateParams()
    {
        var iTwinId = Guid.CreateVersion7();
        var allocation = Factories.MaterialActivityAllocation(iTwinId: iTwinId);
        await _dbContext.Activities.AddAsync(
            allocation.Activity,
            TestContext.Current.CancellationToken
        );
        await _dbContext.Materials.AddAsync(
            allocation.Material,
            TestContext.Current.CancellationToken
        );
        await _dbContext.UnitsOfMeasure.AddAsync(
            allocation.QuantityUnitOfMeasure,
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return new Dictionary<string, object>()
        {
            { "iTwinId", allocation.ITwinId },
            { "activityId", allocation.Activity.Id },
            { "materialId", allocation.Material.Id },
            { "quantityUnitOfMeasureId", allocation.QuantityUnitOfMeasure.Id },
            { "quantityDelta", 5.0m },
            { "progressDate", new LocalDate(2025, 05, 10) },
        };
    }

    [Fact]
    public async Task Create_Accepted()
    {
        var iTwinId = Guid.CreateVersion7();

        var allocation = Factories.MaterialActivityAllocation(iTwinId: iTwinId);
        await _dbContext.Activities.AddAsync(
            allocation.Activity,
            TestContext.Current.CancellationToken
        );
        await _dbContext.Materials.AddAsync(
            allocation.Material,
            TestContext.Current.CancellationToken
        );
        await _dbContext.UnitsOfMeasure.AddAsync(
            allocation.QuantityUnitOfMeasure,
            TestContext.Current.CancellationToken
        );
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = await _client.PostAsyncJson(
            GetPathByName("CreateProgressEntry"),
            new ProgressEntryCreateRequest
            {
                iTwinId = iTwinId,
                activityId = allocation.Activity.Id,
                materialId = allocation.Material.Id,
                quantityUnitOfMeasureId = allocation.QuantityUnitOfMeasure.Id,
                quantityDelta = 5.0m,
                progressDate = new LocalDate(2025, 05, 10),
            },
            TestContext.Current.CancellationToken
        );
        var createdResponse = await response.ShouldBeApiResponse<CreatedResponse<ProgressEntry>>(
            statusCode: HttpStatusCode.Accepted
        );
        createdResponse.Item.Id.ShouldNotBe(Guid.Empty);
    }

    [Theory]
    [InlineData("iTwinId")]
    [InlineData("activityId")]
    [InlineData("materialId")]
    [InlineData("quantityUnitOfMeasureId")]
    public async Task Create_WhenRequiredGuidFieldIsMissing_BadRequest(string missingField)
    {
        var createParams = await BuildCreateParams();
        createParams.Remove(missingField);

        var response = await _client.PostAsyncJson(
            GetPathByName("CreateProgressEntry"),
            createParams,
            TestContext.Current.CancellationToken
        );
        await response.ShouldBeApiErrorBadRequest(
            new Dictionary<string, string[]>()
            {
                { missingField, [$"{missingField} must not be empty."] },
            }
        );
    }

    [Theory]
    [InlineData("activityId", "specified Activity could not be found.")]
    [InlineData("materialId", "specified Material could not be found.")]
    [InlineData(
        "quantityUnitOfMeasureId",
        "specified Quantity Unit of Measure could not be found."
    )]
    public async Task Create_WhenEntityCantBeFound_UnprocessableEntity(
        string missingEntity,
        string errorMessage
    )
    {
        var createParams = await BuildCreateParams();
        createParams[missingEntity] = Guid.CreateVersion7();

        var response = await _client.PostAsyncJson(
            GetPathByName("CreateProgressEntry"),
            createParams,
            TestContext.Current.CancellationToken
        );
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        await response.ShouldBeApiErrorUnprocessableEntity(
            new Dictionary<string, string[]> { { missingEntity, [errorMessage] } }
        );
    }

    [Fact]
    public async Task Create_WhenIdAlreadyUsed_Conflict()
    {
        var existingEntry = Factories.ProgressEntry();
        _dbContext.ProgressEntries.Add(existingEntry);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var createParams = await BuildCreateParams();
        createParams["id"] = existingEntry.Id;

        var response = await _client.PostAsyncJson(
            GetPathByName("CreateProgressEntry"),
            createParams,
            TestContext.Current.CancellationToken
        );
        await response.ShouldBeApiErrorConflict();
    }
}
