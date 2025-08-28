// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSourceToo.Database;
using Kumara.TestCommon;

namespace Kumara.EventSource.Tests;

public class DatabaseTestBase : DatabaseTestBase<ApplicationDbContext>
{
    public override string ConnectionStringName => "KumaraEventSourceDB";
}
