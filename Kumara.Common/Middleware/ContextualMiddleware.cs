// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

// Middleware that only applies sometimes works best implemented with UseWhen. This let's the Middleware control when it's called and without polluting the middleware stack.
public interface IContextualMiddleware
{
    static abstract Func<HttpContext, bool> Predicate { get; }
}

public static class ContextualMiddlewareExtensions
{
    public static IApplicationBuilder UseContextualMiddleware<T>(this IApplicationBuilder app)
        where T : IContextualMiddleware
    {
        return app.UseWhen(T.Predicate, branch => branch.UseMiddleware<T>());
    }
}
