// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace Kumara.Common.Database;

public static class ValidateChangesExtensions
{
    private static readonly ConditionalWeakTable<DbContext, State> _states = new();

    private class State
    {
        public bool SkipValidation { get; set; } = false;
    }

    public static void SetSkipValidation(this DbContext context, bool skipValidation)
    {
        var state = _states.GetOrCreateValue(context);
        state.SkipValidation = skipValidation;
    }

    public static bool ShouldSkipValidation(this DbContext context)
    {
        var state = _states.GetOrCreateValue(context);
        return state.SkipValidation;
    }

    public static int SaveChangesWithoutValidation(this DbContext context)
    {
        context.SetSkipValidation(true);
        try
        {
            return context.SaveChanges();
        }
        finally
        {
            context.SetSkipValidation(false);
        }
    }

    public static async Task<int> SaveChangesWithoutValidationAsync(
        this DbContext context,
        CancellationToken cancellationToken = default
    )
    {
        context.SetSkipValidation(true);
        try
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            context.SetSkipValidation(false);
        }
    }
}
