// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.EventTypes;
using Kumara.TestCommon.Helpers;

namespace Kumara.Common.Tests.EventTypes;

public class ActivityCreatedV1Tests
{
    private ActivityCreatedV1 GetMinimalValidObject() =>
        new()
        {
            Id = Guid.CreateVersion7(),
            Name = "Minimal + Valid",
            ReferenceCode = "MIN001",
        };

    private ActivityCreatedV1 GetFullValidObject() =>
        new()
        {
            Id = Guid.CreateVersion7(),
            Name = "Full + Valid",
            ReferenceCode = "FUL002",
            ControlAccountId = Guid.CreateVersion7(),
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

    [Fact]
    public void BlankReferenceCode_FailsValidation()
    {
        var invalidEventType = GetMinimalValidObject();
        invalidEventType.ReferenceCode = "";

        var results = ModelHelpers.ValidateModel(invalidEventType);
        results.ShouldBeEquivalentTo(new[] { "The ReferenceCode field is required." });
    }
}
