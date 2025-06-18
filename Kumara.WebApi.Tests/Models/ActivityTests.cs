// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.TestCommon.Helpers;
using Kumara.WebApi.Types;

namespace Kumara.WebApi.Tests.Models;

public sealed class ActivityTests : DatabaseTestBase
{
    [Fact]
    public void RoundTripsActualsIntoDatabaseColumns()
    {
        var refActivity = Factories.Activity();
        refActivity.ActualStart = new DateWithOptionalTime
        {
            DateTime = DateTimeOffset.Parse("2001-02-03T04:05:06Z"),
            HasTime = true,
        };
        refActivity.ActualFinish = new DateWithOptionalTime
        {
            DateTime = DateTimeOffset.Parse("2007-08-09T20:11:12+10"),
            HasTime = true,
        };

        var columnValues = DatabaseHelpers.GetEntityColumnValues(_dbContext, refActivity);
        columnValues["actual_start"].ShouldBe(DateTimeOffset.Parse("2001-02-03T04:05:06Z"));
        columnValues["actual_start_has_time"].ShouldBe(true);
        columnValues["actual_finish"].ShouldBe(DateTimeOffset.Parse("2007-08-09T20:11:12+10"));
        columnValues["actual_finish_has_time"].ShouldBe(true);

        var newActivity = Factories.Activity();
        DatabaseHelpers.SetEntityColumnValues(_dbContext, newActivity, columnValues);

        newActivity.ActualStart.ShouldBe(refActivity.ActualStart);
        newActivity.ActualFinish.ShouldBe(refActivity.ActualFinish);

        DatabaseHelpers.SetEntityColumnValues(
            _dbContext,
            newActivity,
            new Dictionary<string, object?>() { { "actual_start_has_time", false } }
        );

        var newActualStart = new DateWithOptionalTime
        {
            DateTime = refActivity.ActualStart.Value.DateTime,
            HasTime = false,
        };

        newActivity.ActualStart.ShouldBe(newActualStart);
        newActivity.ActualFinish.ShouldBe(refActivity.ActualFinish);
    }
}
