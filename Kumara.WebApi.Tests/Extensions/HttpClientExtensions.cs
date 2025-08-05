// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.TestCommon.Helpers;

namespace Kumara.WebApi.Tests;

public static class HttpClientExtensions
{
    public static async Task<HttpResponseMessage> PostAsyncJson<T>(
        this HttpClient client,
        string requestUri,
        T value,
        CancellationToken cancellationToken = default
    )
    {
        var json = JsonSerializer.Serialize(value, AppServicesHelper.JsonSerializerOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return await client.PostAsync(requestUri, content, cancellationToken);
    }
}
