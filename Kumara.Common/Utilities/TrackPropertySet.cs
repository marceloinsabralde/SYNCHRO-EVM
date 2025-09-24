// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Runtime.CompilerServices;

namespace Kumara.Common.Utilities;

public abstract class TrackPropertySet
{
    private readonly HashSet<string> _setProps = new();

    public void SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = "")
    {
        field = newValue;
        _setProps.Add(propertyName);
    }

    public bool HasChanged(string propertyName) => _setProps.Contains(propertyName);
}
