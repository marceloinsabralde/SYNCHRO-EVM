// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Kumara.Common.Database;

public class ITwinIdIndexConvention : IPropertyAddedConvention
{
    public virtual void ProcessPropertyAdded(
        IConventionPropertyBuilder propertyBuilder,
        IConventionContext<IConventionPropertyBuilder> context
    )
    {
        string propertyName = propertyBuilder.Metadata.Name;

        if (propertyName.EndsWith("ITwinId"))
        {
            IConventionEntityType entityType = (IConventionEntityType)
                propertyBuilder.Metadata.DeclaringType;
            entityType.Builder.HasIndex([propertyName]);
        }
    }
}
