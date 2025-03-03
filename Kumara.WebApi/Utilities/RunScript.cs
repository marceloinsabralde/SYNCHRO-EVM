// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.Diagnostics;

namespace Kumara.Utilities;

public class RunScript()
{

    public void Execute(params string[] arguments)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = arguments[0],
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var arg in arguments.Skip(1).ToArray())
        {
            processInfo.ArgumentList.Add(arg);
        }

        using var process = new Process { StartInfo = processInfo };
        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Script execution failed with exit code {process.ExitCode}: {error}");
        }
    }
}
