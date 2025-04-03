// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Reflection;
using CaseConverter;

namespace Kumara.EventSource.Utilities;

public static class EventTypeMapInitializer
{
    public static Dictionary<string, Type> InitializeEventTypeMap()
    {
        Dictionary<string, Type> eventTypeMap = new();

        IEnumerable<Type> eventTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => t is { Namespace: "Kumara.EventSource.Models.Events", IsClass: true });

        foreach (Type type in eventTypes)
        {
            string eventTypeName = $"{Converters.ToKebabCase(type.Name).Replace("-", ".")}";
            eventTypeMap[eventTypeName] = type;
        }

        return eventTypeMap;
    }
}
