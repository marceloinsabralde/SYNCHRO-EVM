// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.WebApi.Types;
using Microsoft.EntityFrameworkCore.Metadata;
using NodaTime;

namespace Kumara.WebApi.Tests.Models;

public sealed class ControlAccountTests : ApplicationTestBase
{
    [Fact]
    public void RoundTripsDatesIntoBackingFields()
    {
        var refControlAccount = Factories.ControlAccount();
        refControlAccount.ActualStart = DateWithOptionalTime.Parse("2001-02-03T04:05:06Z");
        refControlAccount.ActualFinish = DateWithOptionalTime.Parse("2007-08-09T10:11:12+10");
        refControlAccount.PlannedStart = DateWithOptionalTime.Parse("2001-02-03T04:05:06Z");
        refControlAccount.PlannedFinish = DateWithOptionalTime.Parse("2007-08-09T10:11:12+10");

        var currentValues = _dbContext.Entry(refControlAccount).CurrentValues;
        currentValues["_actualStart"]
            .ShouldBe(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2001-02-03T04:05:06Z")));
        currentValues["_actualStartHasTime"].ShouldBe(true);
        currentValues["_actualFinish"]
            .ShouldBe(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2007-08-09T10:11:12+10")));
        currentValues["_actualFinishHasTime"].ShouldBe(true);
        currentValues["_plannedStart"]
            .ShouldBe(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2001-02-03T04:05:06Z")));
        currentValues["_plannedStartHasTime"].ShouldBe(true);
        currentValues["_plannedFinish"]
            .ShouldBe(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2007-08-09T10:11:12+10")));
        currentValues["_plannedFinishHasTime"].ShouldBe(true);

        var newControlAccount = Factories.ControlAccount();
        foreach (var prop in currentValues.Properties)
        {
            _dbContext.Entry(newControlAccount).CurrentValues[prop.Name] = currentValues[prop];
        }

        newControlAccount.ActualStart.ShouldBe(refControlAccount.ActualStart);
        newControlAccount.ActualFinish.ShouldBe(refControlAccount.ActualFinish);
        newControlAccount.PlannedStart.ShouldBe(refControlAccount.PlannedStart);
        newControlAccount.PlannedFinish.ShouldBe(refControlAccount.PlannedFinish);

        _dbContext.Entry(newControlAccount).CurrentValues["_actualStartHasTime"] = false;

        var newActualStart = refControlAccount.ActualStart.Value with { HasTime = false };

        newControlAccount.ActualStart.ShouldBe(newActualStart);
        newControlAccount.ActualFinish.ShouldBe(refControlAccount.ActualFinish);
        newControlAccount.PlannedStart.ShouldBe(refControlAccount.PlannedStart);
        newControlAccount.PlannedFinish.ShouldBe(refControlAccount.PlannedFinish);
    }
}
