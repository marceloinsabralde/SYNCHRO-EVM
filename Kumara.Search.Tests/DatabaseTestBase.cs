// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Elastic.Clients.Elasticsearch;
using Kumara.Search.Database;
using Kumara.TestCommon;

namespace Kumara.Search.Tests;

public class DatabaseTestBase : DatabaseTestBase<ApplicationDbContext>
{
    public override string ConnectionStringName => "KumaraSearchDB";
}
