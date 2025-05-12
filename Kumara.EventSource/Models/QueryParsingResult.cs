// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

namespace Kumara.EventSource.Models;

public class QueryParsingResult
{
    public EventEntityQueryBuilder? QueryBuilder { get; set; }
    public bool IsSuccess => ErrorDetail == null;
    public string? ErrorTitle { get; set; }
    public string? ErrorDetail { get; set; }
    public string? InvalidParameterName { get; set; }
    public int PageSize { get; set; }

    public static QueryParsingResult Success(
        EventEntityQueryBuilder queryBuilder,
        int pageSize = 50
    )
    {
        return new QueryParsingResult { QueryBuilder = queryBuilder, PageSize = pageSize };
    }

    public static QueryParsingResult Failure(string title, string detail, string paramName)
    {
        return new QueryParsingResult
        {
            ErrorTitle = title,
            ErrorDetail = detail,
            InvalidParameterName = paramName,
        };
    }
}
