// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Kumara.EventSource.Extensions;

public static class QueryParametersExtensions
{
    public static QueryParsingResult ToEventEntityQueryBuilder(this IQueryCollection queryParams)
    {
        EventEntityQueryBuilder queryBuilder = new();

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

                default:
                    return QueryParsingResult.Failure(
                        "Unknown Query Parameter",
                        $"Query parameter '{key}' is not supported.",
                        key
                    );
            }
        }

        return QueryParsingResult.Success(queryBuilder);
    }
}
