// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Net.Http.Headers;
using Kumara.TestCommon;
using Kumara.TestCommon.Extensions;
using Kumara.TestCommon.Utilities;
using Kumara.WebApi.Database;
using Microsoft.AspNetCore.Hosting;

namespace Kumara.WebApi.Tests;

public class DatabaseTestBase : DatabaseTestBase<ApplicationDbContext>
{
    public override string ConnectionStringName => "KumaraWebApiDB";

    protected override void ConfigureWebHostBuilder(IWebHostBuilder builder)
    {
        base.ConfigureWebHostBuilder(builder);
        builder.ConfigureTestJwt();
    }
}
