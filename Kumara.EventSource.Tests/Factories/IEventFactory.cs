// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

namespace Kumara.EventSource.Tests.Factories;

/// <summary>
/// Defines the contract for event factories used in tests.
/// </summary>
/// <typeparam name="T">The type of event to create.</typeparam>
public interface IEventFactory<T>
    where T : class
{
    /// <summary>
    /// Creates a valid instance of the event with default test values.
    /// </summary>
    T CreateValid();

    /// <summary>
    /// Creates an invalid instance of the event with problematic test values.
    /// </summary>
    T CreateInvalid();

    /// <summary>
    /// Creates a valid instance and allows customization of specific properties.
    /// </summary>
    /// <param name="customizer">Action to customize the event instance.</param>
    T WithCustomProperties(Action<T> customizer);
}
