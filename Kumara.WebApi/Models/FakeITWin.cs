// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Bentley.ConnectCoreLibs.Providers.Abstractions.ConnectedContextModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kumara.WebApi.Models;

[EntityTypeConfiguration(typeof(FakeITwin.Configuration))]
public class FakeITwin : ITwin
{
    public class Configuration : IEntityTypeConfiguration<FakeITwin>
    {
        public void Configure(EntityTypeBuilder<FakeITwin> builder)
        {
            builder.ToTable("fake_itwins");

            var properties = builder.Metadata.ClrType.GetProperties().Where(p => p.Name != "Id");
            foreach (var property in properties)
            {
                var propertyBuilder = builder.Property(property.Name);
                propertyBuilder.IsRequired(false);
            }
        }
    }
}
