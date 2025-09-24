// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

namespace Kumara.EventSource.Controllers.Requests;

public class EventsCreateRequest
{
    public required List<EventCreateRequest> Events { get; set; }
}
