// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace Kumara.TestCommon.AspNetCore.Controllers;

public sealed class ControllerConventionTests
{
    [Fact]
    public void HasTestData()
    {
        EndpointTestData.ShouldNotBeEmpty();
    }

    [Theory]
    [MemberData(nameof(EndpointTestData))]
    public void EndpointsHaveNameAttributes(string methodName, MethodInfo methodInfo)
    {
        var attribute = methodInfo.GetCustomAttribute<EndpointNameAttribute>();
        attribute.ShouldNotBeNull($"{methodName} is missing [EndpointName]");
        attribute.EndpointName.ShouldMatch(
            "^(List|Get|Create|Update)[A-Z]",
            $"{methodName} endpoint name should start with CRUD action"
        );
    }

    private static IEnumerable<Type> GetApiControllerTypes()
    {
        return typeof(Program)
            .Assembly.GetTypes()
            .Where(type => type.IsClass)
            .Where(type => type.GetCustomAttributes<ApiControllerAttribute>().Any());
    }

    private static IEnumerable<MethodInfo> GetApiControllerMethods()
    {
        return GetApiControllerTypes()
            .SelectMany(type =>
                type.GetMethods(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly
                )
            )
            .Where(method => method.GetCustomAttributes<HttpMethodAttribute>().Any());
    }

    public static IEnumerable<TheoryDataRow<string, MethodInfo>> EndpointTestData =>
        GetApiControllerMethods()
            .Select(methodInfo =>
            {
                var methodName = $"{methodInfo.DeclaringType!.Name}.{methodInfo.Name}";
                var row = new TheoryDataRow<string, MethodInfo>(methodName, methodInfo);
                row.Label = methodName;
                return row;
            });
}
