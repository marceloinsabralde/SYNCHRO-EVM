// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kumara.EventSource.Models;

[EntityTypeConfiguration(typeof(IdempotencyKey.Configuration))]
public class IdempotencyKey
{
    public long Id { get; set; }
    public Guid Key { get; set; }

    public class Configuration : IEntityTypeConfiguration<IdempotencyKey>
    {
        public void Configure(EntityTypeBuilder<IdempotencyKey> builder)
        {
            builder.HasIndex(i => i.Key);
        }
    }
}
