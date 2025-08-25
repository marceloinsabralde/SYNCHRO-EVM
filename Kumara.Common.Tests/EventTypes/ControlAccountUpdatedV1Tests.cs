// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.EventTypes;
using Kumara.TestCommon.Helpers;

namespace Kumara.Common.Tests.EventTypes;

public class ControlAccountUpdatedV1Tests
{
    private ControlAccountUpdatedV1 GetMinimalValidObject() =>
        new() { Id = Guid.CreateVersion7(), Name = "Minimal + Valid" };

    private ControlAccountUpdatedV1 GetFullValidObject() =>
        new()
        {
            Id = Guid.CreateVersion7(),
            Name = "Full + Valid",
            WbsPath = "1.2.3",
            TaskId = Guid.CreateVersion7(),
            PlannedStart = new DateTimeOffset(
                date: new DateOnly(2025, 1, 1),
                time: TimeOnly.MinValue,
                offset: TimeSpan.Zero
            ),
            PlannedFinish = new DateTimeOffset(
                date: new DateOnly(2025, 12, 1),
                time: TimeOnly.MaxValue,
                offset: TimeSpan.Zero
            ),
            ActualStart = new DateTimeOffset(
                date: new DateOnly(2025, 2, 1),
                time: new TimeOnly(hour: 8, minute: 30),
                offset: TimeSpan.Zero
            ),
            ActualFinish = new DateTimeOffset(
                date: new DateOnly(2025, 12, 15),
                time: new TimeOnly(hour: 14, minute: 23),
                offset: TimeSpan.Zero
            ),
        };

    [Fact]
    public void Minimal_PassesValidation()
    {
        var results = ModelHelpers.ValidateModel(GetMinimalValidObject());
        results.ShouldBeEmpty();
    }

    [Fact]
    public void Full_PassesValidation()
    {
        var results = ModelHelpers.ValidateModel(GetFullValidObject());
        results.ShouldBeEmpty();
    }

    [Fact]
    public void BlankName_FailsValidation()
    {
        var invalidEventType = GetMinimalValidObject();
        invalidEventType.Name = "";

        var results = ModelHelpers.ValidateModel(invalidEventType);
        results.ShouldBeEquivalentTo(new[] { "The Name field is required." });
    }
}
