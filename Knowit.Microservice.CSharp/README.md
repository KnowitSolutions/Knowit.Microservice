# ProjectName

TODO: Project description

## Getting Started

The backend is a [gRPC](https://grpc.io/) service hosted on [ASP.NET Core 3.0](https://docs.microsoft.com/en-us/aspnet/?view=aspnetcore-3.0#pivot=core) and written in [C#](https://docs.microsoft.com/en-us/dotnet/csharp/). The frontend is a [React](https://reactjs.org/) application with [TypeScript](https://www.typescriptlang.org/) that is bundled using [Webpack](https://webpack.js.org/).

These instructions will get the project up and running on your local machine for development and testing purposes. 

### Prerequisites

#### Install .NET Core 3.0

Follow the instructions on [this page](https://dotnet.microsoft.com/download/dotnet-core/3.0).

Verify the installation with the following command (version `>=3.0` is required)
```
dotnet --version
```

#### Install the Protobuf compiler

Download and install the latest version [from the GitHub releases page](https://github.com/protocolbuffers/protobuf/releases).

Verify the installation with the following command
```
protoc --version
```

#### Install grpc-web Code Generator Plugin

Download and install the latest version [from the GitHub releases page](https://github.com/grpc/grpc-web/releases)

Verify the installation with the following command
```
protoc-gen-grpc-web -
```
(should throw an error message saying "Unknown option: -")

#### Install Node.js

Download and install the latest version [here](https://nodejs.org/en/).

Verify the installation with the following command
```
node --version
```

#### Development environment
Set the `ASPNETCORE_ENVIRONMENT` environment variable to `Development` to run the project in development mode. In Rider IDE this can be achieved by editing the `Run/Debug Configuration`. Simply add `ASPNETCORE_ENVIRONMENT=Development` to the Environment variables field. [Read more about Run Configurations here](https://blog.jetbrains.com/dotnet/2017/08/23/rundebug-configurations-rider/). If you're using Visual Studio, Visual Studio Code or running from the command line [see this guide](https://andrewlock.net/how-to-set-the-hosting-environment-in-asp-net-core/).

## Running

Run the project from your IDE or with the command

```
dotnet run -p Host
```

### Running the tests

```
dotnet test
```

## Solution structure

This solution is split into multiple projects/directories. This section will explain the intentions behind each project.

| Project | Purpose |
| ------ | ------ |
| [Host](#project-host) | Startup and configuration |
| [Service](#project-service) | gRPC service implementation |
| [Repository](#project-repository) | Database entities |
| [Interface](#project-interface) | React frontend |
| [Contracts](#project-contracts) | Service contracts | 
| [Tests](#project-tests) | Tests for the service | 

### Host <a name="project-host"></a>
The Host project is the entry point of the application and also contains the project configuration. See the [configuration section](#configuration).

### Service <a name="project-service"></a>

The service project contains the implementation of the gRPC service.
 
 #### Implementing the service
 The `Service` class extends the generated base class for the gRPC service defined in `Contracts/ProjectName.proto` and overrides the endpoint methods. A good practice is to split this class into multiple [partial classes](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/partial-classes-and-methods) if the number of service endpoints becomes too large (e.g. one partial class for each entity's CRUD operations).

#### Validation

This project is configured with a gRPC interceptor that will automatically validate request messages for all gRPC endpoints. Validators must be implemented using the [FluentValidation](https://fluentvalidation.net/) library.

Read the [FluentValidation docs](https://fluentvalidation.net/start#creating-your-first-validator) to learn how to create validators. See a simple example in `Service/EchoRequestValidator.cs`.

In order for the validation interceptor to gain access to your validators, they must be added to the service collection.

In `Host/Program.cs`:
```cs 
private static void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddValidator<EchoRequestValidator>();
    ...
}
```

You can also add validators using the FluentValidation ASP.NET integration. See [the documentation](https://fluentvalidation.net/aspnet#getting-started).

### Repository <a name="project-repository"></a>

The repository contains all database entities. It is using [EntityFramework Core 3.0](https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-3.0/features). By default this project uses a local [SQLite](https://www.sqlite.org/index.html) database, see the [configuration section](#configuration) to see how to set up a database connection.

#### Database migrations
Database migrations are used to automatically create, update or delete tables in the database to match the project entities.

Generate migrations
```
 dotnet ef migrations add -s Host -p Repository <name of migration>
```

Apply migrations

```
dotnet ef database update -s Host -p Repository
```

See also
* [EF Core](https://docs.microsoft.com/en-us/ef/core/)
* [EF Core - Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)

### Interface <a name="project-interface"></a>

This project contains the [React](https://reactjs.org/) frontend written in [Typescript](https://www.typescriptlang.org/). It uses [gRPC-web](https://github.com/grpc/grpc-web) to interface with the backend. See examples of this interaction in the `Interface/Hooks` directory.

### Contracts <a name="project-contracts"></a>
The Contracts directory contains `.proto` files that defines the services that are implemented or consumed. All contracts should be placed in this directory. The proto files contains all the service endpoints and messages for the service. Changes to any of the contracts must be applied carefully to ensure backwards compatibility, see the [Proto3 language guide](https://developers.google.com/protocol-buffers/docs/proto3).

During the build process, the `.proto` files in this directory will be compiled into code for both the backend service and the frontend client. For the backend, both client and server code is generated by default (client code for the implemented Service is required when running the tests). To configure this behaviour, see [this documentation](https://docs.microsoft.com/en-us/aspnet/core/grpc/dotnet-grpc?view=aspnetcore-3.0). The Interface project only generates client code.


When there are multiple projects that depend on the same contracts, a good practice is to move the contracts to their own Git repository and add it as a [Git submodule](https://git-scm.com/book/en/v2/Git-Tools-Submodules) in the project that implement or consume any of the contracts.

See also
* [Proto3 language guide](https://developers.google.com/protocol-buffers/docs/proto3) (highly recommended)
* [Introduction to gRPC on .NET Core](https://docs.microsoft.com/en-us/aspnet/core/grpc/?view=aspnetcore-3.0)
* [Protocol Buffer Basics: C#](https://developers.google.com/protocol-buffers/docs/csharptutorial)

### Tests <a name="project-tests"></a>

Includes tests for the gRPC service. Tests are run with their own configuration which can be modified by overriding the `Configure*` methods in the extended `ServiceTests` class. 

Caution:
* By default, the tests are run without the validation interceptor.
* The tests use an in memory database. Entities are **not** automatically removed between each test.

## Configuration <a name="configuration"></a>

### Database configuration

Database connections are configured by providing a `ConnectionString` in `appsettings.json`. [See the documentation](https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-strings#aspnet-core).

```json
{
  "ConnectionStrings": {
    "DatabaseConnection": "Server=projectname.dev;Database=projectname;User ID=projectname;Password=projectname"
  }
}
```

By default, this will create a connection to a Microsoft SQL Server database. To configure this, modify the `ConfigureDatabaseServices` method in `Host/Program.cs`. See [this list of supported database providers](https://docs.microsoft.com/en-us/ef/core/providers/?tabs=dotnet-core-cli).

### External gRPC services 

Connections to external gRPC services are configured in `appsettings.json`. For example, to connect to a service called `Other` on `localhost:5050` add this to `appsettings.json`:

```json
{
  "Grpc": {
    "Clients": {
      "Other": "localhost:5050"
    }
  }
}
```

Make sure you have the contract (`.proto`) for the service in the `Contracts` directory and then build the project.

Add the following to `Host/Program.cs`

```csharp
private static void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddGrpcClientConfiguration<Other.OtherClient>();
    ...
}
```

The client can now be accessed in the `Service` through dependency injection.

#### Elasticsearch

`appsettings.Production.json` contains an incomplete configuration for the Elasticsearch Serilog sink. Make sure to update the `NodeUris` property. [See the documentation for details](https://github.com/serilog/serilog-sinks-elasticsearch).

## Tools

### Bloom RPC

Service endpoints can easily be debugged using [Bloom RPC](https://github.com/uw-labs/bloomrpc) which should be familiar to users of Postman or similar tools for RESTful APIs.

_Get started using Bloom RPC:_

* Download the latest version from [the Github releases page](https://github.com/uw-labs/bloomrpc/releases)
* Install and run
* Import the `.proto` file(s) into Bloom
* Input `localhost:8081` in the address field
* Start sending and receiving requests

## Conventions

### Code style

* [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)
* [Google API Design Guide](https://cloud.google.com/apis/design/)

### Nullable reference types

The project takes advantage of the new C# 8.0 feature, [nullable reference types](https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/nullable-reference-types). You declare a variable to be a nullable reference type by appending a `?` to the type. For example, `string?` represents a nullable `string`. Types with no `?` are never supposed to be `null`. 

