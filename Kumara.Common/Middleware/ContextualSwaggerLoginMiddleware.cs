// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using Bentley.ConnectCoreLibs.Middleware.Abstractions;
using Bentley.ConnectCoreLibs.Middleware.SwaggerLogin;
using Microsoft.AspNetCore.Http;

public class ContextualSwaggerLoginMiddleware : SwaggerLoginMiddleware, IContextualMiddleware
{
    public ContextualSwaggerLoginMiddleware(RequestDelegate n, SwaggerLoginMiddlewareOptions o)
        : base(n, o) { }

    public static Func<HttpContext, bool> Predicate => ctx => IsSwaggerPath(ctx.Request.Path);
}
