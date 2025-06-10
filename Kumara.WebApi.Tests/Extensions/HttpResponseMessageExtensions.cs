// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;

namespace Kumara.WebApi.Tests;

public static class HttpResponseMessageExtensions
{
    private static async Task ShouldBeApiError(
        this HttpResponseMessage response,
        int status,
        string title,
        string type,
        string? errorsPattern = null,
        Dictionary<string, string[]>? errorsDict = null
    )
    {
        response.StatusCode.ShouldBe((HttpStatusCode)status);

        var responseJson = await response.Content.ReadFromJsonAsync<JsonObject>(
            cancellationToken: TestContext.Current.CancellationToken
        );
        responseJson.ShouldNotBeNull();

        List<string> expectedKeys = ["status", "title", "traceId", "type"];

        if (errorsPattern is not null || errorsDict is not null)
        {
            expectedKeys.Add("errors");
        }

        var responseKeys = responseJson.Select(kvp => kvp.Key).ToArray();
        responseKeys.ShouldBe(expectedKeys, ignoreOrder: true);

        responseJson["status"]!.ToString().ShouldBe(status.ToString());
        responseJson["type"]!.ToString().ShouldBe(type);
        responseJson["title"]!.ToString().ShouldBe(title);
        responseJson["traceId"]!.ToString().ShouldNotBeEmpty();

        if (errorsPattern is not null)
        {
            responseJson["errors"]!.ToJsonString().ShouldMatch(errorsPattern);
        }

        if (errorsDict is not null)
        {
            foreach ((string errorKey, string[] errorMessages) in errorsDict)
            {
                responseJson["errors"]![errorKey]!
                    .AsArray()
                    .Select(node => node!.ToString())
                    .ToArray()
                    .ShouldBeEquivalentTo(errorMessages);
            }
        }
    }

    public static async Task ShouldBeApiErrorBadRequest(
        this HttpResponseMessage response,
        string errorsPattern
    )
    {
        await response.ShouldBeApiError(
            status: 400,
            title: "One or more validation errors occurred.",
            type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            errorsPattern: errorsPattern
        );
    }

    public static async Task ShouldBeApiErrorBadRequest(
        this HttpResponseMessage response,
        Dictionary<string, string[]> errorsDict
    )
    {
        await response.ShouldBeApiError(
            status: 400,
            title: "One or more validation errors occurred.",
            type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            errorsDict: errorsDict
        );
    }

    public static async Task ShouldBeApiErrorNotFound(this HttpResponseMessage response)
    {
        await response.ShouldBeApiError(
            status: 404,
            title: "Not Found",
            type: "https://tools.ietf.org/html/rfc9110#section-15.5.5"
        );
    }

    public static async Task ShouldBeApiErrorConflict(this HttpResponseMessage response)
    {
        await response.ShouldBeApiError(
            status: 409,
            title: "Conflict",
            type: "https://tools.ietf.org/html/rfc9110#section-15.5.10"
        );
    }

    private static readonly Lazy<JsonSerializerOptions> _lazyJsonSerializerOptions = new(() =>
    {
        var appFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");
            builder.ConfigureLogging(logging => logging.ClearProviders());
        });

        var mvcOptions = appFactory.Services.GetRequiredService<
            IOptions<Microsoft.AspNetCore.Mvc.JsonOptions>
        >();
        var jsonOptions = mvcOptions.Value.JsonSerializerOptions;

        return jsonOptions;
    });

    public static Task<T?> ShouldBeApiResponse<T>(
        this HttpResponseMessage response,
        HttpStatusCode statusCode = HttpStatusCode.OK
    )
    {
        response.StatusCode.ShouldBe(statusCode);
        response.Content.Headers.ContentType.ShouldNotBeNull();
        response.Content.Headers.ContentType.MediaType.ShouldBe("application/json");

        return response.Content.ReadFromJsonAsync<T>(
            _lazyJsonSerializerOptions.Value,
            TestContext.Current.CancellationToken
        );
    }
}
