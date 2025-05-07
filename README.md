<!-- Copyright (c) Bentley Systems, Incorporated. All rights reserved. -->

# Introduction

Kumara (3579) (formerly SYNCHRO Perform (3221)) is a module of Project Delivery for Bentley Infrastructure Cloud focusing on Progress Capture, Quantity Loaded Resource Planning and Performance Management.

## Project Structure

The solution is broken down into 4 separate projects, these are:

```
Kumara.EventSource << for Event storage and retrieval
Kumara.WebApi << for actual code
Kumara.Tests << for unit tests via MSTest
Kumara.Scenarios << for BDD scenarios
```

## Prerequisites
### General
- [.NET Core SDK Version 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org) note, the homebrew version is suitable.
- [MongoDB](https://www.mongodb.com) (use the docker version).
- [direnv](https://github.com/direnv/direnv) or similar to manage your project directory ENV vars

### Tools

```shell
dotnet tool restore
script/dcl up -d # run services using docker
```

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
This will start the EventSource service on https://localhost:7104 or http://localhost:5220 (check launchSettings.json for the port).

Notes:
- Like the WebApi, you can also use your IDE's "Build and Run" feature to start this service.

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

## Environment Variables

The application uses the following environment variables:

### Connection String Configuration

The application is configured to retrieve connection strings exclusively from environment variables.
This is to ensure that sensitive information, such as database connection strings, are never hard-coded in the source code.

Ensure that the following environment variables are set:

- `ConnectionStrings__KumaraEventSource`: The MongoDB URL connection string for the EventSource database.
     Example: `mongodb://user:password@host:port/database_name?authSource=admin`
       or
     using 1Password CLI
   ```shell
    eval $(op signin)
    op read 'op://E7 Developers/ConnectionStrings__KumaraEventSource/url'
    ```
`
