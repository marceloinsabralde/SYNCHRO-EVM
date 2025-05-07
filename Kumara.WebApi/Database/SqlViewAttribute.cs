namespace Kumara.Database;

[System.AttributeUsage(System.AttributeTargets.Class)]
public class SqlViewAttribute : Attribute
{
    public required string Name;
    public required string SqlFileName;
}
