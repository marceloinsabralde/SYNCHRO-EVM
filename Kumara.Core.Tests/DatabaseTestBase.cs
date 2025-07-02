// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Core.Database;
using Kumara.TestCommon;

namespace Kumara.Core.Tests;

public class DatabaseTestBase : DatabaseTestBase<ApplicationDbContext>
{
    public override string ConnectionStringName => "KumaraCoreDB";
}
