// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Elastic.Clients.Elasticsearch;
using Kumara.Search.Database;
using Kumara.TestCommon;

namespace Kumara.Search.Tests;

[Collection("Non-Parallel Collection")]
public class ApplicationTestBase : ApplicationTestBase<ApplicationDbContext>
{
    public override string ConnectionStringName => "KumaraSearchDB";

    protected ElasticsearchClient elasticsearchClient =>
        _factory!.GetRequiredService<ElasticsearchClient>();

    protected override async Task ResetDatabase()
    {
        await base.ResetDatabase();

        var response = await elasticsearchClient.Indices.DeleteAsync(Indices.All);
        if (!response.IsSuccess())
        {
            throw new InvalidOperationException($"Elasticsearch reset failed: {response}");
        }
    }
}
