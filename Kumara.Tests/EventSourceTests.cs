// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
}
