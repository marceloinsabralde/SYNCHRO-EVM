// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Collections.Concurrent;
using System.Data.Common;
using BraceExpander;
using Kumara.Common.Utilities;
using Kumara.TestCommon.Helpers;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Respawn;
using Respawn.Graph;

namespace Kumara.TestCommon;

public abstract class DatabaseTestBase<T> : IAsyncLifetime
    where T : DbContext
{
    private static readonly ConcurrentDictionary<
        string,
        SharedPool<string>
    > _connectionStringPools = new();
    private string? _connectionString;

    protected AppServicesHelper.AppFactory? _factory;
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

    public abstract string ConnectionStringName { get; }

    public string ConnectionStringTemplate
    {
        get
        {
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables(prefix: "TEST_")
                .Build();

            var templateName = $"{ConnectionStringName}_TEMPLATE";
            var templateValue = config.GetConnectionString(templateName);

            if (templateValue is null)
                throw new InvalidOperationException(
                    $"Could not find connection string template: {templateName}"
                );

            return templateValue;
        }
    }

    public SharedPool<string> ConnectionStringPool =>
        _connectionStringPools.GetOrAdd(
            ConnectionStringTemplate,
            _ =>
            {
                var strings = Expander.Expand(ConnectionStringTemplate);
                return new(strings);
            }
        );

    public async ValueTask InitializeAsync()
    {
        _connectionString = await ConnectionStringPool.CheckoutAsync();

        _factory = AppServicesHelper.CreateWebApplicationFactory(builder =>
        {
            builder.UseSetting($"ConnectionStrings:{ConnectionStringName}", _connectionString);
        });
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

        // Ignore Migration history table otherwise we attempt to migrate
        // each test run and the tables already exist.
        Table[] tablesToIgnore = ["__EFMigrationsHistory"];

        // but don't error if we have not got any other tables yet
        var existingTables = GetTableNames().Select(name => (Table)name).ToHashSet();
        if (existingTables.SetEquals(tablesToIgnore))
            tablesToIgnore = [];

        _respawner = await Respawner.CreateAsync(
            _connection,
            new RespawnerOptions() { DbAdapter = dbAdapter, TablesToIgnore = tablesToIgnore }
        );
    }

    protected IEnumerable<string> GetTableNames()
    {
        using var command = _connection!.CreateCommand();
        command.CommandText =
            @"
            SELECT table_name
            FROM information_schema.tables
            WHERE table_schema = 'public' AND table_type = 'BASE TABLE'
            ";

        using var result = command.ExecuteReader();
        while (result.Read())
        {
            yield return result.GetString(0);
        }
    }

    protected virtual async Task ResetDatabase()
    {
        if (_respawner is not null && _connection is not null)
            await _respawner.ResetAsync(_connection);
    }

    public async ValueTask DisposeAsync()
    {
        await ResetDatabase();
        if (_connectionString is not null)
        {
            await ConnectionStringPool.CheckinAsync(_connectionString);
        }
        _factory?.Dispose();
    }
}
