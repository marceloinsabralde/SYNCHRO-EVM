// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.Security.Claims;
using Bentley.ConnectCoreLibs.Providers.Abstractions.OData;
using Microsoft.AspNetCore.Http;

namespace Kumara.Common.Providers;

public class HttpAuthorizationContext : IAuthorizationContext
{
    private readonly ClaimsPrincipal _user;

    public HttpAuthorizationContext(IHttpContextAccessor accessor)
    {
        _user = accessor.HttpContext?.User ?? new ClaimsPrincipal();
    }

    public string? UserId => _user.Claims.First(c => c.Type == "sub")?.Value;
    public string? Email => _user.Claims.First(c => c.Type == "email")?.Value;

    public bool HasRole(string role) => _user.Claims.Any(c => c.Type == "role" && c.Value == role);
}
