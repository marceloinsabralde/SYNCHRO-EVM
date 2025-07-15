// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Utilities;
using Microsoft.OpenApi.Models;

namespace Kumara.TestCommon.Utilities;

public record SchemaPatcherVisit
{
    public required OpenApiSchema Schema;
    public required Type Type;
}

public class VisitTrackingSchemaPatcher : SchemaPatcher
{
    public List<SchemaPatcherVisit> Visits = new();

    protected override void Patch(OpenApiSchema schema, Type type)
    {
        Visits.Add(new() { Schema = schema, Type = type });
    }
}
