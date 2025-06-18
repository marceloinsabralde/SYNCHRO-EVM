// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Utilities;

namespace Kumara.Common.Tests.Utilities;

public sealed class TrackPropertySetTests
{
    class TestObject : TrackPropertySet
    {
        private int _foo;
        private string? _bar = string.Empty;

        public int Foo
        {
            get => _foo;
            set => SetProperty(ref _foo, value);
        }
        public string? Bar
        {
            get => _bar;
            set => SetProperty(ref _bar, value);
        }
    }

    [Fact]
    public void TracksIfAPropertyHasBeenSet()
    {
        var testObject = new TestObject();

        testObject.HasChanged("Foo").ShouldBeFalse();
        testObject.HasChanged("Bar").ShouldBeFalse();
        testObject.HasChanged("InvalidProp").ShouldBeFalse();

        testObject.Foo = 9;

        testObject.Foo.ShouldBe(9);
        testObject.HasChanged("Foo").ShouldBeTrue();
        testObject.HasChanged("Bar").ShouldBeFalse();

        testObject.Bar = "Twenteen";

        testObject.Bar.ShouldBe("Twenteen");
        testObject.Foo.ShouldBe(9);
        testObject.HasChanged("Foo").ShouldBeTrue();
        testObject.HasChanged("Bar").ShouldBeTrue();
    }
}
