// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.CommandLine;
using System.Diagnostics;
using System.Text.Json;

static string GetPathFromArgs(string[] args)
{
    string? path = null;

    var argument = new Argument<string>(
        "path",
        description: "path to the solution DotSettings.user file"
    );
    var command = new RootCommand("Populates Test Runner environment variables for JetBrains Rider")
    {
        argument,
    };
    command.SetHandler(
        (string input) =>
        {
            path = input;
        },
        argument
    );

    var exitCode = command.Invoke(args);

    if (path is null || exitCode != 0)
    {
        Environment.Exit(exitCode);
    }

    return path;
}

static IDictionary<string, string> GetMiseEnv()
{
    var processStartInfo = new ProcessStartInfo
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true,
    };

    processStartInfo.FileName = "mise";
    processStartInfo.ArgumentList.Add("env");
    processStartInfo.ArgumentList.Add("--json");

    using var process = new Process { StartInfo = processStartInfo };
    process.Start();

    string output = process.StandardOutput.ReadToEnd();
    string error = process.StandardError.ReadToEnd();

    process.WaitForExit();

    if (process.ExitCode != 0)
    {
        throw new Exception(
            $"`mise env` failed with exit code {process.ExitCode}: {output} {error}"
        );
    }

    return JsonSerializer.Deserialize<IDictionary<string, string>>(output)!;
}

var path = GetPathFromArgs(args);
var settings = new StringResourceDictionary();

if (File.Exists(path))
{
    settings.Load(path);
}

var dirty = false;

foreach (var env in GetMiseEnv())
{
    if (env.Key == "PATH")
    {
        continue;
    }

    var escapedKey = env.Key.Replace("_", "_005F").Replace("/", "_002F");
    var settingKey =
        $"/Default/Housekeeping/UnitTestingMru/UnitTestRunner/EnvironmentVariablesIndexed/={escapedKey}/@EntryIndexedValue";

    if (settings[settingKey] != env.Value)
    {
        settings[settingKey] = env.Value;
        dirty = true;
    }
}

if (dirty)
{
    settings.Save(path);
}
