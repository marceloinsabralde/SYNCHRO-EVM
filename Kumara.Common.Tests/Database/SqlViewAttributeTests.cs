// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Reflection;
using Kumara.Common.Database;

namespace Kumara.Common.Tests.Database;

public class SqlViewAttributeTests
{
    [SqlView(Name = "test_class", SqlFileName = "test_class.sql")]
    class TestClassWithSqlViewAttribute;

    class TestClassWithoutSqlViewAttribute;

    [Fact]
    public void AttributeContainsNameAndSqlFile()
    {
        var result = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Select(type => type.GetCustomAttribute<SqlViewAttribute>())
            .Where(attr => attr is not null)
            .ToArray();

        SqlViewAttribute[] expected =
        [
            new SqlViewAttribute { Name = "test_class", SqlFileName = "test_class.sql" },
        ];
        result.ShouldBeEquivalentTo(expected);
    }
}
