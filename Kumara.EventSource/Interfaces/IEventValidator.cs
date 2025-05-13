// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.Models;
using Kumara.EventSource.Utilities;

namespace Kumara.EventSource.Interfaces;

public interface IEventValidator
{
    ValidationResult ValidateEvent(Event @event);
}
