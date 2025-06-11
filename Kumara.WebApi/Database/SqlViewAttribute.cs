// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
namespace Kumara.WebApi.Database;

[System.AttributeUsage(System.AttributeTargets.Class)]
public class SqlViewAttribute : Attribute
{
    public required string Name;
    public required string SqlFileName;
}
