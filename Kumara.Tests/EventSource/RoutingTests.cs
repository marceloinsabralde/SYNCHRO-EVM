// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.Net;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

using Shouldly;

namespace Kumara.Tests.EventSource
{
    [TestClass]
    public sealed class RoutingTests
    {
        private readonly HttpClient _client;

        public RoutingTests()
        {
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Test");
                });
            _client = factory.CreateClient();
        }

        [TestMethod]
        public async Task GetEvents_EndpointIsActive()
        {
            // Act
            var response = await _client.GetAsync("/events");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task PostEvents_EndpointIsActive()
        {
            // Act
            var response = await _client.PostAsync("/events", new StringContent("[]", System.Text.Encoding.UTF8, "application/cloudevents-batch+json"));

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }
}
