// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Kumara.EventSource.Extensions;

public static class QueryParametersExtensions
{
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 200;

    public static QueryParsingResult ToEventQueryBuilder(this IQueryCollection queryParams)
    {
        EventQueryBuilder queryBuilder = new();

        int? pageSize = null;
        string? continuationToken = null;

        foreach (KeyValuePair<string, StringValues> param in queryParams)
        {
            string key = param.Key;
            string value = param.Value.ToString();

            if (string.IsNullOrEmpty(value))
            {
                continue;
            }

            switch (key.ToLowerInvariant())
            {
                case "id":
                    if (Guid.TryParse(value, out Guid id))
                    {
                        queryBuilder.WhereId(id);
                    }
                    else
                    {
                        return QueryParsingResult.Failure(
                            "Invalid Parameter Value",
                            $"Value '{value}' for parameter '{key}' is not a valid GUID.",
                            key
                        );
                    }

                    break;

                case "itwinguid":
                    if (Guid.TryParse(value, out Guid iTwinGuid))
                    {
                        queryBuilder.WhereITwinGuid(iTwinGuid);
                    }
                    else
                    {
                        return QueryParsingResult.Failure(
                            "Invalid Parameter Value",
                            $"Value '{value}' for parameter '{key}' is not a valid GUID.",
                            key
                        );
                    }

                    break;

                case "accountguid":
                    if (Guid.TryParse(value, out Guid accountGuid))
                    {
                        queryBuilder.WhereAccountGuid(accountGuid);
                    }
                    else
                    {
                        return QueryParsingResult.Failure(
                            "Invalid Parameter Value",
                            $"Value '{value}' for parameter '{key}' is not a valid GUID.",
                            key
                        );
                    }

                    break;

                case "correlationid":
                    queryBuilder.WhereCorrelationId(value);
                    break;

                case "type":
                    queryBuilder.WhereType(value);
                    break;

                case "top":
                    if (int.TryParse(value, out int parsedPageSize) && parsedPageSize > 0)
                    {
                        pageSize = Math.Min(parsedPageSize, MaxPageSize);
                    }
                    else
                    {
                        return QueryParsingResult.Failure(
                            "Invalid Parameter Value",
                            $"Value '{value}' for parameter '{key}' is not a valid positive integer.",
                            key
                        );
                    }

                    break;

                case "continuationtoken":
                    continuationToken = value;
                    break;

                default:
                    return QueryParsingResult.Failure(
                        "Unknown Query Parameter",
                        $"Query parameter '{key}' is not supported.",
                        key
                    );
            }
        }

        if (!string.IsNullOrWhiteSpace(continuationToken))
        {
            queryBuilder.WithContinuationToken(continuationToken);
        }

        return QueryParsingResult.Success(queryBuilder, pageSize ?? DefaultPageSize);
    }
}
