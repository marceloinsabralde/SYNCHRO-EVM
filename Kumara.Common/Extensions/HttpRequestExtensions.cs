// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Microsoft.AspNetCore.Http;

namespace Kumara.Common.Extensions;

public static class HttpRequestExtensions
{
    public static string GetUrl(this HttpRequest request, bool withQueryString = true)
    {
        string url = $"{request.Scheme}://{request.Host}{request.Path}";

        if (withQueryString)
            url = $"{url}{request.QueryString}";

        return url;
    }
}
