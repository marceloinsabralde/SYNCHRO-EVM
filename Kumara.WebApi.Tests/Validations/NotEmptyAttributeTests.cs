// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using Kumara.Validations;

namespace Kumara.WebApi.Tests.Validations;

public sealed class NotEmptyAttributeTests
{
    [Fact]
    public void null_NotEmptyValidation()
    {
        var attr = new NotEmptyAttribute();

        attr.IsValid(null).ShouldBeTrue();
    }

    [Fact]
    public void String_NotEmptyValidation()
    {
        var attr = new NotEmptyAttribute();

        attr.IsValid("").ShouldBeFalse();
        attr.IsValid("foo").ShouldBeTrue();
    }

    [Fact]
    public void Guid_NotEmptyValidation()
    {
        var attr = new NotEmptyAttribute();

        attr.IsValid(Guid.Empty).ShouldBeFalse();
        attr.IsValid(Guid.CreateVersion7()).ShouldBeTrue();
    }

    [Fact]
    public void UnsupportedType()
    {
        var attr = new NotEmptyAttribute();
        Should.Throw<ArgumentException>(() => attr.IsValid(0));
    }
}
