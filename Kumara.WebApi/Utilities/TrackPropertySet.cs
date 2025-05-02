using System.Runtime.CompilerServices;

namespace Kumara.Utilities;

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
