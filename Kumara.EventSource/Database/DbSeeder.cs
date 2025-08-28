// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.Json;
using Kumara.EventSource.Models;
using Microsoft.EntityFrameworkCore;

namespace Kumara.EventSource.Database;

public static class DbSeeder
{
    public static void SeedData(DbContext context, bool _)
    {
        SeedDataAsync(context, _, CancellationToken.None).GetAwaiter().GetResult();
    }

    public static async Task SeedDataAsync(
        DbContext context,
        bool _,
        CancellationToken cancellationToken
    )
    {
        if (!await context.Set<Event>().AnyAsync(cancellationToken))
        {
            await context.Set<Event>().AddRangeAsync(Events, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static readonly List<Guid> AccountITwinIds =
    [
        new("A0000000-0000-0000-0000-000000000001"),
        new("A0000000-0000-0000-0000-000000000002"),
    ];

    private static readonly List<Guid> ProjectITwinIds =
    [
        new("B0000000-0000-0000-0000-000000000001"),
        new("B0000000-0000-0000-0000-000000000002"),
    ];

    private static readonly List<Event> Events =
    [
        new()
        {
            ITwinId = ProjectITwinIds[0],
            AccountId = AccountITwinIds[0],
            Id = new Guid("E0000000-0000-0000-0000-000000000001"),
            Type = "controlaccount.created.v1",
            Data = JsonSerializer.SerializeToDocument(
                new { Id = Guid.CreateVersion7(), Name = "Test Control Account 1" },
                JsonSerializerOptions.Web
            ),
        },
        new()
        {
            ITwinId = ProjectITwinIds[0],
            AccountId = AccountITwinIds[0],
            Id = new Guid("E0000000-0000-0000-0000-000000000002"),
            Type = "activity.created.v1",
            Data = JsonSerializer.SerializeToDocument(
                new
                {
                    Id = Guid.CreateVersion7(),
                    Name = "Test Activity 1",
                    ReferenceCode = "ACT001",
                },
                JsonSerializerOptions.Web
            ),
        },
        new()
        {
            ITwinId = ProjectITwinIds[1],
            AccountId = AccountITwinIds[1],
            Id = new Guid("E0000000-0000-0000-0000-000000000003"),
            Type = "controlaccount.created.v1",
            Data = JsonSerializer.SerializeToDocument(
                new { Id = Guid.CreateVersion7(), Name = "Test Control Account 2" },
                JsonSerializerOptions.Web
            ),
        },
    ];
}
