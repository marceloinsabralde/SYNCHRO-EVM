// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.Text.Json;

using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Http;
using CloudNative.CloudEvents.SystemTextJson;

using Microsoft.AspNetCore.Mvc.Testing;

using Shouldly;

namespace Kumara.Tests
{
    [TestClass]
    public sealed class WeatherForecastTests
    {
        private readonly HttpClient _client;

        public WeatherForecastTests()
        {
            var factory = new WebApplicationFactory<Program>();
            _client = factory.CreateClient();
        }

        [TestMethod]
        public async Task GetWeatherForecast_ReturnsSuccessStatusCode()
        {
            // Arrange
            var request = "/weatherforecast";

            // Act
            var response = await _client.GetAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            responseString.ShouldNotBeNull();
            responseString.ShouldContain("TemperatureC");
        }
    }

    [TestClass]
    public sealed class EventSourceTests
    {
        private readonly HttpClient _client;
        private readonly String _endpoint = "/events";

        public EventSourceTests()
        {
            var factory = new WebApplicationFactory<Program>();
            _client = factory.CreateClient();
        }

        [TestMethod]
        public async Task PostEvents_WithZeroEvents_ReturnsSuccessAndCountZero()
        {
            // Act
            var content = new StringContent("[]", System.Text.Encoding.UTF8, "application/cloudevents-batch+json");
            var response = await _client.PostAsync(_endpoint, content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            responseString.ShouldNotBeNull();
            responseString.ShouldContain("\"count\":0");
        }


        [TestMethod]
        public async Task PostEvents_WithMultipleEvents_ReturnsSuccessAndCorrectCount()
        {
            var eventsPayload = new List<CloudEvent>
            {
                new CloudEvent(CloudEventsSpecVersion.V1_0)
                {
                    Type = "UserLogin",
                    Source = new Uri("/source/user"),
                    Id = "A234-1234-1234",
                    Time = DateTimeOffset.Parse("2023-10-01T12:00:00Z"),
                    Data = new
                    {
                        userId = "12345",
                        userName = "arun.malik"
                    }
                },
                new CloudEvent(CloudEventsSpecVersion.V1_0)
                {
                    Type = "FileUpload",
                    Source = new Uri("/source/file"),
                    Id = "B234-1234-1234",
                    Time = DateTimeOffset.Parse("2023-10-01T12:05:00Z"),
                    Data = new
                    {
                        userId = "12345",
                        fileName = "report.pdf",
                        fileSize = 102400
                    }
                }
            };

            // Act
            var formatter = new JsonEventFormatter();
            var encodedContent = formatter.EncodeBatchModeMessage(eventsPayload, out var contentType);

            var content = new ByteArrayContent(encodedContent.ToArray());
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType.MediaType);
            var response = await _client.PostAsync(_endpoint, content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            responseString.ShouldNotBeNull();
            responseString.ShouldContain("\"count\":2");
        }
    }
}
