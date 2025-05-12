// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSource.Models;
using Kumara.EventSource.Models.Events;

namespace Kumara.Tests.EventSource;

public static class EventRepositoryTestUtils
{
    public static List<EventEntity> GetTestEventEntities()
    {
        return new List<EventEntity>
        {
            new()
            {
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "kumara.test.event",
                DataJson = JsonSerializer.SerializeToDocument(
                    new TestCreatedV1
                    {
                        TestString = "Test String",
                        TestEnum = TestOptions.OptionA,
                        TestInteger = 100,
                    }
                ),
            },
            new()
            {
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "some.random.event",
                DataJson = JsonSerializer.SerializeToDocument(
                    new
                    {
                        RandomString = "RandomValue",
                        RandomNumber = 42,
                        NestedObject = new
                        {
                            NestedKey = "NestedValue",
                            NestedArray = new[] { 1, 2, 3 },
                        },
                    }
                ),
            },
            new()
            {
                ITwinGuid = Guid.NewGuid(),
                AccountGuid = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Source = new Uri("http://testsource.com"),
                Type = "other.random.event",
                DataJson = JsonSerializer.SerializeToDocument(
                    new
                    {
                        RandomKey = "RandomValue",
                        NestedObject = new
                        {
                            NestedKey = "NestedValue",
                            NestedArray = new[] { 1, 2, 3 },
                        },
                        RandomNumber = 42,
                    }
                ),
            },
        };
    }
}
