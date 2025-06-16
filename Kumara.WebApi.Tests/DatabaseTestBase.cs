// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Data.Common;
using Kumara.WebApi.Database;
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
    private readonly WebApplicationFactory<Program> _factory;
    protected readonly HttpClient _client;
    protected readonly ApplicationDbContext _dbContext;

    private Respawner? _respawner;
    private DbConnection? _connection;

    public DatabaseTestBase()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");
            builder.ConfigureLogging(logging => logging.ClearProviders());
        });

        _client = _factory.CreateClient();
        _dbContext = _factory.Services.GetRequiredService<ApplicationDbContext>();
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
        _factory.Dispose();
    }
}
