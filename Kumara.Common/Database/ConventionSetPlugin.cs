// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Kumara.Common.Database;

public class ConventionSetPlugin : IConventionSetPlugin
{
    public ConventionSet ModifyConventions(ConventionSet conventionSet)
    {
        conventionSet.PropertyAddedConventions.Add(new ITwinIdIndexConvention());
        conventionSet.PropertyAddedConventions.Add(new ITwinIdNamingConvention());

        conventionSet.ModelFinalizingConventions.Add(new EnumToJsonStringValueConvention());

        return conventionSet;
    }
}
