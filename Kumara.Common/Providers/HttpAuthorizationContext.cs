// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Kumara.Common.Providers;

public class HttpAuthorizationContext(IHttpContextAccessor accessor) : IAuthorizationContext
{
    public ClaimsPrincipal User => accessor.HttpContext?.User ?? new ClaimsPrincipal();

    public string? UserId => User.Claims.First(c => c.Type == "sub")?.Value;
    public string? Email => User.Claims.First(c => c.Type == "email")?.Value;
    public string? ItwinId => accessor.HttpContext?.GetRouteData()?.Values["itwinId"]?.ToString();

    public bool HasRole(string role) => User.Claims.Any(c => c.Type == "role" && c.Value == role);
}
