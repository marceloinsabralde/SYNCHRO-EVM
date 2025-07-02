// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.Elasticsearch;

namespace Kumara.Search.Tests;

public class IntegrationTestAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string TestUsername = "elastic";
    private const string TestPassword = "elasticsearchpassword";
    public ElasticsearchContainer ElasticsearchContainer;

    public IntegrationTestAppFactory()
    {
        ElasticsearchContainer = new ElasticsearchBuilder()
            .WithImage("elasticsearch:9.0.2")
            .WithEnvironment("xpack.security.enabled", "true")
            .WithEnvironment("ELASTIC_PASSWORD", TestPassword)
            .WithEnvironment("discovery.type", "single-node")
            .WithEnvironment("ES_JAVA_OPTS", "-Xms512m -Xmx512m")
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            var testConfig = new Dictionary<string, string?>
            {
                {
                    "ConnectionStrings:KumaraSearchES",
                    $"http://{TestUsername}:{TestPassword}@{ElasticsearchContainer.Hostname}:{ElasticsearchContainer.GetMappedPublicPort(9200)}"
                },
            };
            configurationBuilder.AddInMemoryCollection(testConfig);
        });

        // Override the Elasticsearch client registration to use the test container with authentication
        builder.ConfigureServices(services =>
        {
            // Remove existing registration and replace with test version
            services.RemoveAll(typeof(ElasticsearchClient));
            services.AddSingleton(_ =>
            {
                var elasticsearchUrl = new Uri(
                    $"http://{TestUsername}:{TestPassword}@{ElasticsearchContainer.Hostname}:{ElasticsearchContainer.GetMappedPublicPort(9200)}"
                );
                var settings = new ElasticsearchClientSettings(elasticsearchUrl);
                return new ElasticsearchClient(settings);
            });
        });
    }

    public async ValueTask InitializeAsync()
    {
        await ElasticsearchContainer.StartAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await ElasticsearchContainer.DisposeAsync().ConfigureAwait(false);
        await base.DisposeAsync().ConfigureAwait(false);
    }
}
