// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.EventSource.Models;
using Kumara.EventSource.Utilities;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Kumara.EventSource.DbContext;

public class MongoDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public MongoDbContext(DbContextOptions<MongoDbContext> options)
        : base(options)
    {
        // Inform EF Core that we are using MongoDB that does not support transactions
        Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
    }

    public DbSet<EventEntity> Events { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<EventEntity>(entity =>
        {
            entity.ToCollection("events");

            entity.Property(e => e.Id);
            entity.Property(e => e.ITwinGuid);
            entity.Property(e => e.AccountGuid);
            entity.Property(e => e.CorrelationId);
            entity.Property(e => e.SpecVersion);
            entity
                .Property(e => e.Source)
                .HasConversion(uri => uri.ToString(), str => new Uri(str));
            entity.Property(e => e.Type);
            entity
                .Property(e => e.DataJson)
                .HasConversion(new JsonDocumentConverter())
                .HasElementName("data_json");
        });
    }
}
