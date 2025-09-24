// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net.Http.Headers;
using Kumara.EventSource.Database;
using Kumara.TestCommon;
using Kumara.TestCommon.Extensions;
using Kumara.TestCommon.Utilities;
using Microsoft.AspNetCore.Hosting;

namespace Kumara.EventSource.Tests;

public class ApplicationTestBase : ApplicationTestBase<ApplicationDbContext>
{
    public override string ConnectionStringName => "KumaraEventSourceDB";

    protected override void ConfigureWebHostBuilder(IWebHostBuilder builder)
    {
        base.ConfigureWebHostBuilder(builder);
        builder.ConfigureTestJwt();
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            StubTokenGenerator.GenerateToken()
        );
    }
}
