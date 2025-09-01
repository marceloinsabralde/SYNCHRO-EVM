// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Kumara.Common.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration;

namespace Kumara.EventSource.Models;

[EntityTypeConfiguration(typeof(Event.Configuration))]
public class Event : ITimestampedEntity, IPageableEntity, IDisposable
{
    public Guid Id { get; set; }

    public required Guid ITwinId { get; set; }

    public required Guid AccountId { get; set; }

    public string? CorrelationId { get; set; }

    public required string Type { get; set; }

    public Guid? TriggeredByUserSubject { get; set; }

    public Instant? TriggeredByUserAt { get; set; }

    public required JsonDocument Data { get; set; }

    public Instant CreatedAt { get; set; }

    [NotMapped]
    public Instant UpdatedAt { get; set; }

    public class Configuration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.HasKey(e => new { e.Id, e.AccountId });
            // Generate a Guid for Id as it's no longer the primary key
            builder.Property(e => e.Id).HasValueGenerator<NpgsqlSequentialGuidValueGenerator>();
        }
    }

    public void Dispose()
    {
        Data.Dispose();
    }
}
