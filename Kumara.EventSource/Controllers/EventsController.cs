// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSource.Extensions;
using Kumara.EventSource.Interfaces;
using Kumara.EventSource.Models;
using Kumara.EventSource.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Kumara.EventSource.Controllers;

[ApiController]
[Route("api/v1/events")]
public class EventsController : ControllerBase
{
    private const string ContinuationTokenKey = "continuationtoken";
    private readonly IEventRepository _eventRepository;
    private readonly IEventValidator _eventValidator;

    public EventsController(IEventRepository eventRepository, IEventValidator eventValidator)
    {
        _eventRepository = eventRepository;
        _eventValidator = eventValidator;
    }

    [HttpPost]
    public async Task<IActionResult> PostEvents(
        [FromBody] JsonElement payload,
        CancellationToken cancellationToken = default
    )
    {
        if (payload.ValueKind != JsonValueKind.Array)
        {
            return BadRequest(
                new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid Payload",
                    Detail = "The payload must be an array of JSON objects.",
                    Instance = HttpContext.Request.Path,
                }
            );
        }

        List<Event> events =
            JsonSerializer.Deserialize<List<Event>>(
                payload.GetRawText(),
                KumaraJsonOptions.DefaultOptions
            ) ?? [];

        foreach (Event @event in events)
        {
            EventValidationResult eventValidationResult = await _eventValidator.ValidateEventAsync(
                @event,
                cancellationToken
            );
            if (!eventValidationResult.IsValid)
            {
                return BadRequest(eventValidationResult.Errors);
            }
        }

        if (events.Count != 0)
        {
            await _eventRepository.AddEventsAsync(events, cancellationToken);
        }

        return Ok(new { count = events.Count });
    }

    [HttpGet]
    public async Task<IActionResult> GetEvents(
        [FromQuery(Name = ContinuationTokenKey)] string? continuationToken = null,
        CancellationToken cancellationToken = default
    )
    {
        IQueryCollection query = HttpContext.Request.Query;

        QueryParsingResult queryParsingResult;

        if (!string.IsNullOrEmpty(continuationToken))
        {
            Pagination.ContinuationToken? token = Pagination.ParseContinuationToken(
                continuationToken
            );
            if (token != null)
            {
                QueryCollection tokenParams = new(
                    token.QueryParameters.ToDictionary(
                        kvp => kvp.Key,
                        kvp => new StringValues(kvp.Value)
                    )
                );

                Dictionary<string, StringValues> paramsWithToken = new(
                    tokenParams.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                );
                paramsWithToken[ContinuationTokenKey] = continuationToken;

                queryParsingResult = new QueryCollection(paramsWithToken).ToEventQueryBuilder();
            }
            else
            {
                return BadRequest(
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Invalid Continuation Token",
                        Detail = "The provided continuation token is invalid or malformed.",
                        Instance = HttpContext.Request.Path,
                        Extensions = { ["invalidParameter"] = ContinuationTokenKey },
                    }
                );
            }
        }
        else
        {
            queryParsingResult = query.ToEventQueryBuilder();
        }

        if (!queryParsingResult.IsSuccess)
        {
            return BadRequest(
                new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = queryParsingResult.ErrorTitle,
                    Detail = queryParsingResult.ErrorDetail,
                    Instance = HttpContext.Request.Path,
                    Extensions = { ["invalidParameter"] = queryParsingResult.InvalidParameterName },
                }
            );
        }

        PaginatedList<Event> paginatedList = await _eventRepository.GetPaginatedEventsAsync(
            queryParsingResult.QueryBuilder!,
            queryParsingResult.PageSize,
            cancellationToken
        );

        BuildPaginationLinks(paginatedList, query, queryParsingResult.QueryBuilder!);

        return Ok(paginatedList);
    }

    private void BuildPaginationLinks<T>(
        PaginatedList<T> paginatedList,
        IQueryCollection query,
        EventQueryBuilder queryBuilder
    )
    {
        HttpRequest request = HttpContext.Request;
        string baseUrl = $"{request.Scheme}://{request.Host}{request.Path}";

        Dictionary<string, string> queryParams = new();

        if (queryBuilder.TokenQueryParameters.Count > 0)
        {
            foreach (KeyValuePair<string, string> param in queryBuilder.TokenQueryParameters)
            {
                queryParams[param.Key] = param.Value;
            }
        }
        else
        {
            foreach (KeyValuePair<string, StringValues> param in query)
            {
                if (
                    param.Key.Equals(
                        ContinuationTokenKey,
                        StringComparison.InvariantCultureIgnoreCase
                    ) || string.IsNullOrEmpty(param.Value.ToString())
                )
                {
                    continue;
                }

                string value = param.Value.ToString();
                queryParams[param.Key] = value;
            }
        }

        string? continuationToken = null;
        if (paginatedList.Items.Count > 0 && paginatedList.HasMoreItems)
        {
            T? lastItem = paginatedList.Items.LastOrDefault();
            if (lastItem != null)
            {
                Guid lastItemId = ((dynamic)lastItem).Id;

                continuationToken = Pagination.CreateContinuationToken(lastItemId, queryParams);
            }
        }

        paginatedList.SetPaginationLinks(
            paginatedList.HasMoreItems,
            continuationToken,
            baseUrl,
            queryParams
        );
    }
}

public class QueryCollection(Dictionary<string, StringValues> dictionary)
    : Dictionary<string, StringValues>(dictionary, StringComparer.OrdinalIgnoreCase),
        IQueryCollection
{
    ICollection<string> IQueryCollection.Keys => GetKeys();

    private List<string> GetKeys()
    {
        return base.Keys.ToList();
    }
}
