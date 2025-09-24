// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace Kumara.Common.Extensions;

public static class HttpRequestExtensions
{
    public static string GetUrl(this HttpRequest request, bool withQueryString = true)
    {
        string url = $"{request.Scheme}://{request.Host}{request.Path}";

        if (withQueryString)
            url = QueryHelpers.AddQueryString(url, request.Query);

        return url;
    }
}
