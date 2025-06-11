<!-- Copyright (c) Bentley Systems, Incorporated. All rights reserved. -->

# Introduction

Kumara (3579) (formerly SYNCHRO Perform (3221)) is a module of Project Delivery for Bentley Infrastructure Cloud focusing on Progress Capture, Quantity Loaded Resource Planning and Performance Management.

## Project Structure

The solution is broken down into the following projects:

```
Kumara.TestCommon << to test common functionality
Kumara.Common << for common functionality used across the Kumara projects
Kumara.Common.Tests << for unit tests to test Kumara.Common
Kumara.EventSource << for Event storage and retrieval
Kumara.EventSource.Tests << for unit tests to test Kumara.EventSource
Kumara.Core << for event sourced entities and functionality
Kumara.WebApi << exposes API for all of Kumara project and POC entities via CRUD
Kumara.WebApi.Tests << for unit tests to test Kumara.WebApi
Kumara.Scenarios << for BDD scenarios
```

## Prerequisites
### General
- [.NET Core SDK Version 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- A container runtime such as Podman or Docker, along with Docker Compose for container orchestration.
  - Tip: `brew install podman podman-desktop docker docker-compose` will get you close.
  - Tip: Check out [`podman-machine-run`](https://github.com/jasoncodes/dotfiles/blob/master/bin/podman-machine-run) (and accompanying [`launchd.plist`](https://github.com/jasoncodes/dotfiles/blob/master/LaunchAgents/podman-machine.plist)) to automate creation and starting of Podman Machine on macOS.
- [mise](https://mise.jdx.dev) to manage environment variables.

### Tools

```shell
mise trust
dotnet tool restore
script/dcl up --wait # run services using docker
```

### JetBrains Rider

Rider users should install the Mise plugin per the instructions [here](https://github.com/134130/intellij-mise).

Note that this plugin does not currently automatically set environment variables when using the Test Runner
(i.e. running an individual test vs the entire test project). To workaround this you can run `mise run rider-test-env`
to populate the environment variables in Settings > Build, Execution, Deployment > Unit Testing > Test Runner > Environment variables.

It is recommended to configure this as a Startup Task to ensure the Test Runner always has the current configuration:

1. Navigate to Rider > Settings > Tools > Startup Tasks
2. Click the "+", "Add New Configuration", then "Mise Toml Task"
3. Set "Name" to "Mise Test Runner environment"
4. Set "Mise task name" to "rider-test-env"
5. Uncheck "Activate tool window"
6. Click "OK" then "Save"
7. Close and re-open the project to test

## Running Tests

From the root directory of the solution run `dotnet test`, you should see an output similar to this:

```
Test summary: total: 2, failed: 0, succeeded: 1, skipped: 1, duration: 2.1s
Build succeeded with 8 warning(s) in 3.0s
```

Notes:
- if you just want to run the MSTest (unit tests), cd into `Kumara.Tests` then re-run `dotnet test`
- if you just want to run the Reqnroll (BDD scenarios), cd into `Kumara.Scenarios` then re-run `dotnet test`
- you may wish to simply use the 'Play Button' style feature of your favourite IDE for this.

## Running the apps

### Running Kumara.WebApi

From the root directory of the solution run `dotnet run --project Kumara.WebApi`
Then visit https://localhost:7029/swagger/index.html to view in browser.

Notes:
- you may wish to simply use the "Build and Run" feature of your favourite IDE for this.

### Running Kumara.EventSource

From the root directory of the solution run `dotnet run --project Kumara.EventSource`
Then visit https://localhost:7104/swagger/index.html to view in browser.

Notes:
- Like the WebApi, you can also use your IDE's "Build and Run" feature to start this service.

### Running Kumara.Core

From the root directory of the solution run `dotnet run --project Kumara.Core`
Then visit https://localhost:7133 to view in browser.

#### Using the HTTP Test File

The repository includes a `Kumara.EventSource.http` file that can be used to manually test the EventSource API endpoints.

##### Using with JetBrains Rider
1. Open the `Kumara.EventSource.http` file in Rider
2. You'll see a green "Run" icon next to each HTTP request
3. Click the icon to execute that specific request
4. View the response in the "Response" panel that appears
5. [Learn more about using HTTP Client in Rider](https://www.jetbrains.com/help/rider/Http_client_in__product__code_editor.html)

##### Using with Visual Studio Code
1. Install the "REST Client" extension by Huachao Mao
2. Open the `Kumara.EventSource.http` file in VS Code
3. You'll see a "Send Request" link above each request
4. Click the link to execute that specific request
5. View the response in a split-view panel that opens
6. [Learn more about the REST Client extension](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)

## How to view code coverage

- Locate the build
- Click on 'Test' tab to see test results.
- Click on 'Code Coverage' tab to see and download code coverage report.

## Formatting

We're using [CSharpier](https://csharpier.com) to format the .cs files in this repo. CSharpier is included in our tools manifest should be installed if you used the `dotnet tool restore` command above.

```shell
dotnet csharpier . # format all files in the current directory
dotnet csharpier --check . # check that all files in the current directory are formatted according to csharpier's rules
```

## Databases

Use `mongosh "${ConnectionStrings__KumaraEventSourceDB?}"` to connect to Event Source's MongoDB database.
Use `psql kumara-web-api` to connect to Web API's PostgreSQL database.
See `mise.toml` for a list of environment variables if you want to use other database tools.
