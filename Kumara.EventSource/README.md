# Kumara.EventSource

## Overview

Kumara.EventSource is a core component of the SYNCHRO EVM (Kumara) project, responsible for handling the
event sourcing capabilities of the application. It provides an API for storing, retrieving, and querying events related
to construction project management.

## Target Framework

- .NET 9.0
- ASP.NET Core 9.0

## External Libraries

- **MongoDB.Driver (v3.3.0)**: Official .NET driver for MongoDB.
- **MongoDB.EntityFrameworkCore (v9.0.0)**: Integration of MongoDB with Entity Framework Core.
- **Microsoft.AspNetCore.OpenApi**: Support for OpenAPI documentation.
- **NJsonSchema (v11.2.0)**: JSON Schema reader, generator and validator for .NET.
- **CaseConverter (v2.0.1)**: Utility for converting between various case conventions.

## Architecture

The EventSource module follows Clean Architecture principles and is organized as follows:

### Controllers

- `EventsController`: Exposes RESTful API endpoints for posting and querying events.
    - POST /events: Accepts an array of events and validates them before storage.
    - GET /events: Retrieves events based on various query parameters with pagination support.

### Models

- `Event`: Core domain model representing an event in the system with properties like:
    - ITwinId: Identifier for the digital twin of a construction project.
    - AccountId: Account identifier.
    - CorrelationId: Used to track related events.
    - Id: Unique identifier for the event.
    - SpecVersion: The version of the CloudEvents spec.
    - Source: URI identifying the context in which an event occurred.
    - Type: Classification of the event.
    - DataJson: JSON document containing event-specific information.
- `EventQueryBuilder`: Builder pattern implementation for constructing event queries.
- `PaginatedEvents`: Model for paginated event results.
- `QueryParsingResult`: Result from parsing query parameters.

### Interfaces

- `IEventRepository`: Defines methods for event data access operations.
- `IEventValidator`: Defines validation logic for events.

### Repositories

- `EventRepositoryInMemoryList`: In memory implementation of the event repository interface.
- `EventRepositoryMongo`: MongoDB implementation of the event repository interface.

### DbContext

- MongoDB DbContext configuration for Entity Framework Core.

### Utilities

- `JsonSchemaGenerator`: Generates JSON schemas for event models.
- `EventTypeMapInitializer`: Initializes mapping between event type strings and their corresponding .NET types.
- `EventValidator`: Validates events against their schemas.
- `GuidUtility`: Utilities for handling GUIDs.
- `Pagination`: Utilities for handling pagination with continuation tokens.

### Extensions

- Query parameter parsing extensions
- Repository extensions

## Implementation Notes

1. **Event Validation**: Events are validated against JSON schemas that are auto-generated from .NET model classes.

2. **Query Capabilities**: The API supports querying events by:
    - ITwinId
    - AccountId
    - CorrelationId
    - Event Type
    - Other custom queries via query parameters

3. **Pagination**: The API implements pagination using continuation tokens, allowing clients to efficiently retrieve
   large result sets.

4. **MongoDB Integration**: Uses MongoDB as the event store, leveraging Entity Framework Core for data access.

5. **Error Handling**: Implements appropriate error responses with ProblemDetails objects.

6. **Configuration**: Configurable through standard ASP.NET Core appsettings.json mechanism.

## Getting Started

### Prerequisites

- .NET SDK 9.0 or higher
- MongoDB instance

### Running the Service

(If the environment variable is set)

```bash
dotnet run --project Kumara.EventSource
```

## API Usage Examples

See Kumara.EventSource/Kumara.EventSource.http for a collection of HTTP requests to test the API.

### Post Events

```http
POST /Events
Content-Type: application/json

[
  {
    "iTwinId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "accountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "correlationId": "correlation123",
    "id": "6fa85f64-5717-4562-b3fc-2c963f66afa7",
    "specVersion": "1.0",
    "source": "https://example.com/source",
    "type": "ProjectCreated",
    "data": {
      "projectName": "Example Project",
      "startDate": "2025-01-01"
    }
  }
]
```

### Query Events

```http
GET /Events?iTwinId=3fa85f64-5717-4562-b3fc-2c963f66afa6&type=test.created.v1
```

### Paginated Query

```http
GET /Events?iTwinId=3fa85f64-5717-4562-b3fc-2c963f66afa6&pagesize=50
```

### Using Continuation Token

```http
GET /Events?continuationtoken=eyJsYXN0SWQiOiI2ZmE4NWY2NC01NzE3LTQ1NjItYjNmYy0yYzk2M2Y2NmFmYTciLCJxdWVyeVBhcmFtZXRlcnMiOnsiaXR3aW5ndWlkIjoiM2ZhODVmNjQtNTcxNy00NTYyLWIzZmMtMmM5NjNmNjZhZmE2In19
```

## Response Format

The GET /Events endpoint returns a paginated response with the following structure:

```json
{
  "items": [
    // array of event objects
  ],
  "totalCount": 157,
  "pageSize": 50,
  "hasMoreItems": true,
  "links": {
    "self": "https://api.example.com/events?iTwinId=3fa85f64-5717-4562-b3fc-2c963f66afa6&pagesize=50",
    "next": "https://api.example.com/events?continuationtoken=eyJsYXN0SWQiOiI2ZmE4NWY2NC01NzE3LTQ1NjItYjNmYy0yYzk2M2Y2NmFmYTciLCJxdWVyeVBhcmFtZXRlcnMiOnsiaXR3aW5ndWlkIjoiM2ZhODVmNjQtNTcxNy00NTYyLWIzZmMtMmM5NjNmNjZhZmE2In19"
  }
}
```
