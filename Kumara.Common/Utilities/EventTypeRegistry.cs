// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Reflection;
using Kumara.Common.Attributes;

namespace Kumara.Common.Utilities;

public class EventTypeRegistry
{
    private static readonly Lazy<EventTypeRegistry> _instance = new(() => new EventTypeRegistry());

    public static EventTypeRegistry Instance => _instance.Value;

    private EventTypeRegistry()
    {
        _registry = GenerateRegistry();
    }

    private readonly Dictionary<string, Type> _registry;

    public bool IsValidEventType(string eventTypeName) => _registry.ContainsKey(eventTypeName);

    public bool TryGetEventType(string eventTypeName, out Type? eventType) =>
        _registry.TryGetValue(eventTypeName, out eventType);

    public IEnumerable<string> GetEventTypes() => _registry.Keys;

    private Dictionary<string, Type> GenerateRegistry()
    {
        return Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.GetCustomAttribute<EventTypeAttribute>() is not null)
            .ToDictionary(
                type => type.GetCustomAttribute<EventTypeAttribute>()!.Name,
                type => type
            );
    }
}
