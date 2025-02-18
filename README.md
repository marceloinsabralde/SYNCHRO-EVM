<!-- Copyright (c) Bentley Systems, Incorporated. All rights reserved. -->
# Introduction
SYNCHRO Perform NextGen (3579) (formerly SYNCHRO Perform (3221)) is a module of Project Delivery for Bentley Infrastructure Cloud focusing on Progress Capture, Quantity Loaded Resource Planning and Performance Management.

## Project Structure
The solution is broken down into 3 separate projects, these are:

```
SYNCHROPerformNextGen.WebApi << for actual code
SYNCHROPerformNextGen.Tests << for unit tests via MSTest
SYNCHROPerformNextGen.Scenarios << for BDD scenarios
```

## Prerequisites
### General
- [.NET Core SDK Version 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org) note, the homebrew version is suitable.
- [direnv](https://github.com/direnv/direnv) or similar to manage your project directory ENV vars

### Tools

```shell
- dotnet tool install -g dotnet-ef # entity-framework tools
- dotnet tool install -g dotnet-outdated-tool # tool to update deps
```

## Running Tests

From the root directory of the solution run `dotnet test`, you should see an output similar to this:

```
Test summary: total: 2, failed: 0, succeeded: 1, skipped: 1, duration: 2.1s
Build succeeded with 8 warning(s) in 3.0s
```

Notes:
- if you just want to run the MSTest (unit tests), cd into `SYNCHROPerformNextGen.Tests` then re-run `dotnet test`
- if you just want to run the Reqnroll (BDD scenarios), cd into `SYNCHROPerformNextGen.Scenarios` then re-run `dotnet test`
- you may wish to simply use the 'Play Button' style feature of your favourite IDE for this.

## Running the app

From the root directory of the solution run `dotnet run --project SYNCHROPerformNextGen.WebApi`
Then visit https://localhost:7029/swagger/index.html to view in browser.

Notes:
- you may wish to simply use the "Build and Run" feature of your favourite IDE for this.

## How to view code coverage

- Locate the build
- Click on 'Test' tab to see test results.
- Click on 'Code Coverage' tab to see and download code coverage report.
