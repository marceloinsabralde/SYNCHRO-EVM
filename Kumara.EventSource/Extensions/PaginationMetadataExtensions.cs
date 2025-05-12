// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.Models;

namespace Kumara.EventSource.Extensions;

public static class PaginationMetadataExtensions
{
    private const string BaseUrl = "http://api.kumara.com";

    public static void SetPaginationLinks<T>(
        this PaginatedList<T> paginatedList,
        bool hasMoreItems,
        string? continuationToken,
        string baseUrl = BaseUrl,
        Dictionary<string, string>? queryParameters = null
    )
    {
        ArgumentNullException.ThrowIfNull(paginatedList);

        paginatedList.HasMoreItems = hasMoreItems;

        if (paginatedList.Links == null)
        {
            paginatedList.Links = new PaginationLinks();
        }

        string queryString = BuildQueryString(queryParameters);

        paginatedList.Links.Self = new PaginationLink { Href = $"{baseUrl}{queryString}" };

        if (hasMoreItems && !string.IsNullOrEmpty(continuationToken))
        {
            string nextQueryString =
                $"?continuationToken={Uri.EscapeDataString(continuationToken)}";

            if (queryParameters != null && queryParameters.TryGetValue("top", out string? topValue))
            {
                nextQueryString += $"&top={Uri.EscapeDataString(topValue)}";
            }

            paginatedList.Links.Next = new PaginationLink { Href = $"{baseUrl}{nextQueryString}" };
        }
        else
        {
            paginatedList.Links.Next = null;
        }
    }

    private static string BuildQueryString(Dictionary<string, string>? parameters)
    {
        if (parameters == null || parameters.Count == 0)
        {
            return string.Empty;
        }

        List<string> queryParams = parameters
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}")
            .ToList();

        return $"?{string.Join("&", queryParams)}";
    }
}
