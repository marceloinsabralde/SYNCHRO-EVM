// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Shouldly;

namespace Kumara.Tests;

[TestClass]
public sealed class ExampleTest
{
    [TestMethod]
    public void TestMethod1()
    {
        true.ShouldBe(true);
    }
}
