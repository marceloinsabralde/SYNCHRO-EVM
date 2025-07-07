// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;
using Bentley.ConnectCoreLibs.Providers.Abstractions.ConnectedContextModels;
using Bentley.ConnectCoreLibs.Providers.Abstractions.Interfaces;
using Bentley.ConnectCoreLibs.Providers.Abstractions.OData;
using Kumara.WebApi.Database;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kumara.WebApi.Providers;

class FakeITwinProvider(ApplicationDbContext dbContext) : IITwinProvider
{
    public async Task<ITwinResponse> GetAsync(
        Guid iTwinId,
        ODataRequestOptions? requestOptions = null,
        Dictionary<string, string>? requestHeaders = null
    )
    {
        var iTwin = await dbContext.FakeITwins.FindAsync(iTwinId);

        if (iTwin is null)
        {
            return new()
            {
                StatusCode = HttpStatusCode.NotFound,
                Error = new()
                {
                    Code = "iTwinNotFound",
                    Message = "Requested iTwin is not available.",
                },
            };
        }

        return new() { StatusCode = HttpStatusCode.OK, iTwin = iTwin };
    }

    public ILogger Logger { get; set; } = NullLogger.Instance;
    public bool LogBeforeRequest { get; set; } = false;
    public bool LogAfterRequest { get; set; } = false;
    public Func<HttpRequestMessage, Task> OnBeforeRequest { get; set; } =
        context => Task.CompletedTask;

    public Task<ITwinsResponse> GetAsync(
        string subClass,
        ODataRequestOptions? requestOptions = null,
        Dictionary<string, string>? requestHeaders = null
    )
    {
        throw new NotImplementedException();
    }

    public Task<ITwinResponse> GetPrimaryAccountAsync(
        Dictionary<string, string>? requestHeaders = null
    )
    {
        throw new NotImplementedException();
    }

    public Task<ITwinResponse> CreateAsync(
        ITwin iTwin,
        Dictionary<string, string>? requestHeaders = null
    )
    {
        throw new NotImplementedException();
    }

    public Task<ITwinResponse> PatchAsync(
        Guid iTwinId,
        ITwin iTwin,
        Dictionary<string, string>? requestHeaders = null
    )
    {
        throw new NotImplementedException();
    }

    public Task<BaseODataResponse> DeleteAsync(
        Guid iTwinId,
        Dictionary<string, string>? requestHeaders = null
    )
    {
        throw new NotImplementedException();
    }

    public Task<DataCentersResponse> GetDataCenters(
        Dictionary<string, string>? requestHeaders = null
    )
    {
        throw new NotImplementedException();
    }

    public Task<DataCenterResponse> GetDataCenter(
        Guid dataCenterId,
        Dictionary<string, string>? requestHeaders = null
    )
    {
        throw new NotImplementedException();
    }

    public Task<TResponse> GetAsync<TResponse>(
        string url,
        ODataRequestOptions? requestOptions = null,
        Dictionary<string, string>? requestHeaders = null
    )
        where TResponse : BaseODataResponse, new()
    {
        throw new NotImplementedException();
    }

    public Task<TResponse> PostAsync<TResponse>(
        string url,
        ITwin entity,
        Dictionary<string, string>? requestHeaders = null
    )
        where TResponse : BaseODataResponse, new()
    {
        throw new NotImplementedException();
    }

    public Task<TResponse> PatchAsync<TResponse>(
        string url,
        ITwin entity,
        Dictionary<string, string>? requestHeaders = null
    )
        where TResponse : BaseODataResponse, new()
    {
        throw new NotImplementedException();
    }

    public Task<TResponse> DeleteAsync<TResponse>(
        string url,
        Dictionary<string, string>? requestHeaders = null
    )
        where TResponse : BaseODataResponse, new()
    {
        throw new NotImplementedException();
    }
}
