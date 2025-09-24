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
        PaginationLink? nextPage = null;

        if (result.HasMore)
        {
            filter.ContinueFromId = result.LastReadId;
            var continuationToken = new ContinuationToken<TQueryFilter>(filter);

            // Build new query params based on existing ones to ensure we don't drop any required params (e.g iTwinId)
            var queryParams = QueryHelpers.ParseQuery(controller.Request.QueryString.Value);
            queryParams["$continuationToken"] = continuationToken.ToBase64String();

            nextPage = new(
                QueryHelpers.AddQueryString(
                    controller.Request.GetUrl(withQueryString: false),
                    queryParams
                )
            );
        }

        return new PaginatedListResponse<TResponse>()
        {
            Items = items,
            Links = new() { Self = new(controller.Request.GetUrl()), Next = nextPage },
        };
    }
}
