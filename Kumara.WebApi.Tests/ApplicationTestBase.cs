// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net.Http.Headers;
using Kumara.TestCommon;
using Kumara.TestCommon.Extensions;
using Kumara.TestCommon.Utilities;
using Kumara.WebApi.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Kumara.WebApi.Tests;

public class ApplicationTestBase : ApplicationTestBase<ApplicationDbContext>
{
    public override string ConnectionStringName => "KumaraWebApiDB";

    protected override void ConfigureWebHostBuilder(IWebHostBuilder builder)
    {
        base.ConfigureWebHostBuilder(builder);
        builder.ConfigureTestJwt();
        builder.ConfigureTestServices(services =>
        {
            services.AddTransient<
                Bentley.ConnectCoreLibs.Providers.Abstractions.Interfaces.IITwinProvider,
                Kumara.WebApi.Providers.FakeITwinProvider
            >();
        });
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
