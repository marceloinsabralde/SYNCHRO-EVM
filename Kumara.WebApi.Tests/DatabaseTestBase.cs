// Copyright (c) Bentley Systems, Incorporated. All rights reserved.
using System.Data.Common;
using Kumara.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Respawn;

namespace Kumara.WebApi.Tests;

[Collection("Non-Parallel Collection")]
public class DatabaseTestBase : IAsyncLifetime
{
    protected readonly HttpClient _client;

    protected readonly ApplicationDbContext _dbContext;

    private Respawner? _respawner;
    private DbConnection? _connection;

    public DatabaseTestBase()
    {
        var appFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");
            builder.ConfigureLogging(logging => logging.ClearProviders());
        });

        _client = appFactory.CreateClient();
        _dbContext = appFactory.Services.GetService<ApplicationDbContext>()!;
    }

    public async ValueTask InitializeAsync()
    {
        _connection = _dbContext.Database.GetDbConnection();
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(
            _connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                // Ignore Migration history table otherwise we attempt to migrate
                // each test run and the tables already exist.
                TablesToIgnore = ["__EFMigrationsHistory"],
            }
        );
    }

    protected async Task ResetDatabase()
    {
        if (_respawner is not null && _connection is not null)
            await _respawner.ResetAsync(_connection);
    }

    public async ValueTask DisposeAsync()
    {
        await ResetDatabase();
    }
}
