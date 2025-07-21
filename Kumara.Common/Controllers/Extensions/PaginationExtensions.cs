// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Controllers.Responses;
using Kumara.Common.Extensions;
using Kumara.Common.Queries;
using Kumara.Common.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Kumara.Common.Controllers.Extensions;

public static class PaginationExtensions
{
    public static PaginatedListResponse<TResponse> BuildPaginatedResponse<TResponse, TQueryFilter>(
        this ControllerBase controller,
        IEnumerable<TResponse> items,
        IPageableQueryResult result,
        TQueryFilter filter
    )
        where TResponse : notnull
        where TQueryFilter : IPageableQueryFilter
    {
        var currentUrl = controller.Request.GetUrl();
        PaginationLink? nextPage = null;

        if (result.HasMore)
        {
            filter.ContinueFromId = result.LastReadId;
            var continuationToken = new ContinuationToken<TQueryFilter>(filter);
            nextPage = new(
                QueryHelpers.AddQueryString(
                    currentUrl, // If exclude the existing query params, we drop iTwinId which is required
                    "$continuationToken",
                    continuationToken.ToBase64String()
                )
            );
        }

        return new PaginatedListResponse<TResponse>()
        {
            Items = items,
            Links = new() { Self = new(currentUrl), Next = nextPage },
        };
    }
}
