// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Data.Common;
using Kumara.TestCommon.Helpers;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Respawn;

namespace Kumara.TestCommon;

[Collection("Non-Parallel Collection")]
public class DatabaseTestBase<T> : IAsyncLifetime
    where T : DbContext
{
    private AppServicesHelper.AppFactory? _factory;
    protected HttpClient _client = null!;
    protected T _dbContext = null!;
    protected LinkGenerator _linkGenerator = null!;

    private Respawner? _respawner;
    private DbConnection? _connection;

    public string GetPathByName(string endpointName, object? values = null)
    {
        var path = _linkGenerator.GetPathByName(endpointName, values);
        if (path is null)
        {
            throw new ArgumentException($"Could not find path for {endpointName}");
        }
        return path;
    }

    public async ValueTask InitializeAsync()
    {
        _factory = AppServicesHelper.CreateWebApplicationFactory();
        _client = _factory.CreateClient();
        _dbContext = _factory.GetRequiredService<T>();
        _linkGenerator = _factory.GetRequiredService<LinkGenerator>();

        _connection = _dbContext.Database.GetDbConnection();
        await _connection.OpenAsync();

        var dbAdapter = _connection.GetType().Name switch
        {
            "NpgsqlConnection" => DbAdapter.Postgres,
            "SqlConnection" => DbAdapter.SqlServer,
            _ => throw new ArgumentException("Unknown database adapter"),
        };

        _respawner = await Respawner.CreateAsync(
            _connection,
            new RespawnerOptions
            {
                DbAdapter = dbAdapter,
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
        _factory?.Dispose();
    }
}
