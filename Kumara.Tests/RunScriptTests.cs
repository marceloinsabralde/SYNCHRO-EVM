// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using Shouldly;

namespace Kumara.Utilities;

[TestClass]
public class RunScriptTests
{
    [TestMethod]
    public void Execute_SuccessfulScript_Succeeds()
    {
        var runScript = new RunScript();
        runScript.Execute("bash", "-c", "echo test");
    }

    [TestMethod]
    public void Execute_FailingScript_ThrowsExceptionWithErrorMessage()
    {
        var runScript = new RunScript();
        var exception = Should.Throw<Exception>(
            () => runScript.Execute("bash", "-c", "echo test >&2; false")
        );
        exception.Message.ShouldContain("test");
    }
}
