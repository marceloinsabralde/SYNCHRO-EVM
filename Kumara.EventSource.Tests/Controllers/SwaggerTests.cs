// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Kumara.EventSource.Tests.Controllers;

public class SwaggerTests
{
    private readonly WebApplicationFactory<Program> _factory;

    public SwaggerTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            // Configure the test server to always enable Swagger
            builder.UseEnvironment("Development");
        });
    }

    [Fact]
    public async Task SwaggerEndpoint_ShouldBeAccessible()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(
            "/swagger/index.html",
            CancellationToken.None
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync(CancellationToken.None);

        content.ShouldContain("swagger-ui");
        content.ShouldContain("Swagger UI");
    }

    [Fact]
    public async Task SwaggerJsonEndpoint_ShouldReturnOpenApiDocument()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(
            "/swagger/v1/swagger.json",
            CancellationToken.None
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");

        string content = await response.Content.ReadAsStringAsync(CancellationToken.None);
        content.ShouldContain("openapi");
        content.ShouldContain("paths");
    }
}
