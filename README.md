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

___

This README is a work in progress, everything below this line is initial boilerplate to be filled out.
___

# Getting Started
TODO: Guide users through getting your code up and running on their own system. In this section you can talk about:
1.	Installation process
2.	Software dependencies
3.	Latest releases
4.	API references

# Build and Test
TODO: Describe and show how to build your code and run the tests. 

# Contribute
TODO: Explain how other users and developers can contribute to make your code better. 

If you want to learn more about creating good readme files then refer the following [guidelines](https://docs.microsoft.com/en-us/azure/devops/repos/git/create-a-readme?view=azure-devops). You can also seek inspiration from the below readme files:
- [ASP.NET Core](https://github.com/aspnet/Home)
- [Visual Studio Code](https://github.com/Microsoft/vscode)
- [Chakra Core](https://github.com/Microsoft/ChakraCore)