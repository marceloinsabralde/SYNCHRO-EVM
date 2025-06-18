// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Kumara.Common.Database;

public class ITwinIdNamingConvention : IPropertyAddedConvention
{
    private readonly Regex pattern = new Regex(@"(?<=^|_)i_twin(?=_)");

    public virtual void ProcessPropertyAdded(
        IConventionPropertyBuilder propertyBuilder,
        IConventionContext<IConventionPropertyBuilder> context
    )
    {
        string oldColumnName = propertyBuilder.Metadata.GetColumnName();
        string newColumnName = pattern.Replace(oldColumnName, "itwin");

        if (oldColumnName != newColumnName)
        {
            propertyBuilder.HasColumnName(newColumnName);
        }
    }
}
