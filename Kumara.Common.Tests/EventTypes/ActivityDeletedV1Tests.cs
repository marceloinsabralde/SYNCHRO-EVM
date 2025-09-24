// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.EventTypes;
using Kumara.TestCommon.Helpers;

namespace Kumara.Common.Tests.EventTypes;

public class ActivityDeletedV1Tests
{
    private ActivityDeletedV1 GetValidObject() => new() { };

    [Fact]
    public void PassesValidation()
    {
        var results = ModelHelpers.ValidateModel(GetValidObject());
        results.ShouldBeEmpty();
    }
}
