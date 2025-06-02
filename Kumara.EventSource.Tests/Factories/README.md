# Event Factory Pattern for Test Data Generation

## Overview

This directory contains factory classes that provide a consistent and maintainable way to generate test data for events.
The factories follow a common interface and encapsulate the logic for creating both valid and invalid test instances.

## Factory Classes

- `ControlAccountCreatedV1Factory`: Creates test instances of control account creation events
- `RandomEventFactory`: Creates test instances of generic events with random data

## Usage Example

```csharp
// Creating a valid event
var factory = new ControlAccountCreatedV1Factory();
var validEvent = factory.CreateValid();

// Creating an invalid event for negative testing
var invalidEvent = factory.CreateInvalid();

// Customizing event properties
var customEvent = factory.WithCustomProperties(evt => {
    evt.ITwinId = Guid.NewGuid();
    evt.CorrelationId = "custom-id";
});
```

## Interface

All factories implement the `IEventFactory<T>` interface:

```csharp
public interface IEventFactory<T> where T : class
{
    T CreateValid();
    T CreateInvalid();
    T WithCustomProperties(Action<T> customizer);
}
```

## Benefits

- **Maintainability**: Test data generation is centralized and consistent
- **Reusability**: Factory methods can be used across multiple test classes
- **Type Safety**: Generic interface ensures type-safe event creation
- **Extensibility**: New event types can be easily added by implementing the interface

## Guidelines

1. Keep factory methods focused on creating a specific type of event
2. Ensure both valid and invalid data scenarios are covered
3. Use meaningful defaults that represent common test cases
4. Allow customization through the WithCustomProperties method
5. Document any special conditions or assumptions in the factory code

## Integration with Test Utilities

The factories are integrated with `EventRepositoryTestUtils` to provide test data for repository tests:

```csharp
public static class EventRepositoryTestUtils
{
    private static readonly ControlAccountCreatedV1Factory _controlAccountFactory = new();
    private static readonly RandomEventFactory _randomEventFactory = new();

    public static List<Event> GetTestEvents()
    {
        return new List<Event>
        {
            _controlAccountFactory.CreateValid(),
            _randomEventFactory.WithCustomProperties(evt => evt.Type = "some.random.event"),
            _randomEventFactory.WithCustomProperties(evt => evt.Type = "other.random.event"),
        };
    }
}
