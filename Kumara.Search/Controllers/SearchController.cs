// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Elastic.Clients.Elasticsearch;
using Kumara.Common.Controllers.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Kumara.Search.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SearchController(ElasticsearchClient elasticsearchClient) : ControllerBase
{
    private readonly ElasticsearchClient _elasticsearchClient = elasticsearchClient;

    [HttpGet]
    [EndpointName("ListSearch")]
    public async Task<ActionResult<ListResponse<object>>> Search(
        [FromQuery] string q,
        CancellationToken cancellationToken
    )
    {
        var searchQuery = string.IsNullOrWhiteSpace(q) ? "*" : q;
        const string index = "documents";

        await EnsureIndexAsync(index);

        var response = await _elasticsearchClient.SearchAsync<object>(
            s => s.Indices(index).Query(qs => qs.QueryString(qsq => qsq.Query(searchQuery))),
            cancellationToken
        );

        if (!response.IsValidResponse)
            return StatusCode(500, new { error = response.ElasticsearchServerError?.ToString() });

        var listResponse = new ListResponse<object>
        {
            Items = [.. response.Hits.Select(h => h.Source ?? new object())],
        };
        return Ok(listResponse);
    }

    private async Task EnsureIndexAsync(string index)
    {
        var checkIndex = await _elasticsearchClient.Indices.ExistsAsync(index);

        if (!checkIndex.Exists)
        {
            var createResponse = await _elasticsearchClient.Indices.CreateAsync(index);
            if (!createResponse.IsValidResponse)
                throw new Exception(
                    createResponse.ElasticsearchServerError?.ToString() ?? "Failed to create index"
                );
        }
    }
}
