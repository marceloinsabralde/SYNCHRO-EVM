// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Kumara.Common.Database;

public class ValidateChangesInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        if (eventData.Context is not null && !eventData.Context.ShouldSkipValidation())
        {
            var entities = ChangedEntities(eventData.Context);
            ValidateChangedEntities(entities);
        }

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.Context is not null && !eventData.Context.ShouldSkipValidation())
        {
            var entities = ChangedEntities(eventData.Context);
            ValidateChangedEntities(entities);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static IEnumerable<object> ChangedEntities(DbContext context)
    {
        return context
            .ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .Select(e => e.Entity);
    }

    private static void ValidateChangedEntities(IEnumerable<object> entities)
    {
        foreach (var entity in entities)
        {
            var validationContext = new ValidationContext(entity);
            Validator.ValidateObject(entity, validationContext, validateAllProperties: true);
        }
    }
}
