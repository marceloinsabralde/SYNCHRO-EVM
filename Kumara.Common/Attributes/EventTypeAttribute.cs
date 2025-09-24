// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

namespace Kumara.Common.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class EventTypeAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
