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

    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<Channel> Channels { get; set; } = null!;
    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<EntityType> EntityTypes { get; set; } = null!;
    public DbSet<EventType> EventTypes { get; set; } = null!;
    public DbSet<PublishAuthority> PublishAuthorities { get; set; } = null!;
    public DbSet<ReadAuthority> ReadAuthorities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToCollection("events");

            entity.Property(e => e.Id);
            entity.Property(e => e.ITwinId);
            entity.Property(e => e.AccountId);
            entity.Property(e => e.CorrelationId);
            entity.Property(e => e.IdempotencyId);
            entity.Property(e => e.SpecVersion);
            entity
                .Property(e => e.Source)
                .HasConversion(uri => uri.ToString(), str => new Uri(str));
            entity.Property(e => e.Type);
            entity.Property(e => e.Time);
            entity
                .Property(e => e.DataJson)
                .HasConversion(new JsonDocumentConverter())
                .HasElementName("data_json");
        });

        modelBuilder.Entity<Channel>(entity =>
        {
            entity.ToCollection("channels");
            entity.Property(e => e.Id);
            entity.Property(e => e.Code);
            entity.Ignore(e => e.EntityTypes);
            entity.Ignore(e => e.EventTypes);
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToCollection("clients");
            entity.Property(e => e.Id);
            entity.Property(e => e.IMSClientId);
            entity.Property(e => e.GPRID);
            entity.Ignore(e => e.ReadAuthorities);
            entity.Ignore(e => e.PublishAuthorities);
        });

        modelBuilder.Entity<EntityType>(entity =>
        {
            entity.ToCollection("entityTypes");
            entity.Property(e => e.Id);
            entity.Property(e => e.Code);
            entity.Property(e => e.ChannelId);
            entity.Ignore(e => e.Channel);
            entity.Ignore(e => e.EventTypes);
        });

        modelBuilder.Entity<EventType>(entity =>
        {
            entity.ToCollection("eventTypes");
            entity.Property(e => e.Id);
            entity.Property(e => e.Action);
            entity.Property(e => e.Version);
            entity.Property(e => e.Schema);
            entity.Property(e => e.ChannelId);
            entity.Property(e => e.EntityTypeId);
            entity.Ignore(e => e.Channel);
            entity.Ignore(e => e.EntityType);
            entity.Ignore(e => e.Events);
        });

        modelBuilder.Entity<PublishAuthority>(entity =>
        {
            entity.ToCollection("publishAuthorities");
            entity.Property(e => e.Id);
            entity.Property(e => e.ClientId);
            entity.Property(e => e.ChannelId);
            entity.Ignore(e => e.Client);
            entity.Ignore(e => e.Channel);
        });

        modelBuilder.Entity<ReadAuthority>(entity =>
        {
            entity.ToCollection("readAuthorities");
            entity.Property(e => e.Id);
            entity.Property(e => e.ClientId);
            entity.Property(e => e.ChannelId);
            entity.Ignore(e => e.Client);
            entity.Ignore(e => e.Channel);
        });
    }
}
