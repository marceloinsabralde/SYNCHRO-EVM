// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.WebApi.Types;
using NodaTime;

namespace Kumara.WebApi.Tests.Models;

public sealed class ActivityTests : ApplicationTestBase
{
    [Fact]
    public void RoundTripsDatesIntoBackingFields()
    {
        var refActivity = Factories.Activity();
        refActivity.ActualStart = new DateWithOptionalTime
        {
            DateTime = OffsetDateTime.FromDateTimeOffset(
                DateTimeOffset.Parse("2001-02-03T04:05:06Z")
            ),
            HasTime = true,
        };
        refActivity.ActualFinish = new DateWithOptionalTime
        {
            DateTime = OffsetDateTime.FromDateTimeOffset(
                DateTimeOffset.Parse("2007-08-09T20:11:12+10")
            ),
            HasTime = true,
        };
        refActivity.PlannedStart = DateWithOptionalTime.Parse("2001-02-03T04:05:06Z");
        refActivity.PlannedFinish = DateWithOptionalTime.Parse("2007-08-09T20:11:12+10");

        var currentValues = _dbContext.Entry(refActivity).CurrentValues;
        currentValues["_actualStart"]
            .ShouldBe(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2001-02-03T04:05:06Z")));
        currentValues["_actualStartHasTime"].ShouldBe(true);
        currentValues["_actualFinish"]
            .ShouldBe(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2007-08-09T20:11:12+10")));
        currentValues["_actualFinishHasTime"].ShouldBe(true);
        currentValues["_plannedStart"]
            .ShouldBe(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2001-02-03T04:05:06Z")));
        currentValues["_plannedStartHasTime"].ShouldBe(true);
        currentValues["_plannedFinish"]
            .ShouldBe(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2007-08-09T20:11:12+10")));
        currentValues["_plannedFinishHasTime"].ShouldBe(true);

        var newActivity = Factories.Activity();
        foreach (var prop in currentValues.Properties)
        {
            _dbContext.Entry(newActivity).CurrentValues[prop.Name] = currentValues[prop];
        }

        newActivity.ActualStart.ShouldBe(refActivity.ActualStart);
        newActivity.ActualFinish.ShouldBe(refActivity.ActualFinish);
        newActivity.PlannedStart.ShouldBe(refActivity.PlannedStart);
        newActivity.PlannedFinish.ShouldBe(refActivity.PlannedFinish);

        _dbContext.Entry(newActivity).CurrentValues["_actualStartHasTime"] = false;

        var newActualStart = new DateWithOptionalTime
        {
            DateTime = refActivity.ActualStart.Value.DateTime,
            HasTime = false,
        };

        newActivity.ActualStart.ShouldBe(newActualStart);
        newActivity.ActualFinish.ShouldBe(refActivity.ActualFinish);
        newActivity.PlannedStart.ShouldBe(refActivity.PlannedStart);
        newActivity.PlannedFinish.ShouldBe(refActivity.PlannedFinish);
    }
}
