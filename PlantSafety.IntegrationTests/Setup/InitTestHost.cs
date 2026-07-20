// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Setup
//  Layer:          IntegrationTests
//  File:           InitTestHost.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    WebApplicationFactory test host for PlantSafety API integration tests.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using PlantSafety.Infrastructure.Common.Configuration;

namespace PlantSafety.IntegrationTests.Setup;

/// <summary>
/// <see cref="WebApplicationFactory{TEntryPoint}"/> that boots the PlantSafety API
/// against a Testcontainers SQL Server instance.  One instance is shared across all
/// tests in the collection via <see cref="IntegrationTestCollection"/>.
/// </summary>
public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    /// <summary>SQL connection string pointing at the Testcontainers instance.</summary>
    public string ConnectionString => _sqlContainer.GetConnectionString();

    /// <inheritdoc/>
    public async ValueTask InitializeAsync()
    {
        await _sqlContainer.StartAsync();
        await TestDatabaseFactory.ApplySchemaAsync(ConnectionString);
        await TestDataSeeder.SeedAsync(ConnectionString);
    }

    /// <inheritdoc/>
    public new async ValueTask DisposeAsync()
    {
        await _sqlContainer.DisposeAsync();
    }

    /// <inheritdoc/>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTest");

        // DatabaseOptions properties are init-only (deliberate immutability per the
        // architecture rules), so the Testcontainers connection string must be supplied
        // as a configuration override BEFORE binding, not by mutating an already-bound
        // instance via services.Configure(...).
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{DatabaseOptions.SectionName}:{nameof(DatabaseOptions.ConnectionString)}"] = ConnectionString,
                [$"{DatabaseOptions.SectionName}:{nameof(DatabaseOptions.ReadConnectionString)}"] = ConnectionString,
            });
        });
    }
}

/// <summary>xunit v3 collection fixture — shares the test host across all tests in the assembly.</summary>
[CollectionDefinition(nameof(IntegrationTestCollection))]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebAppFactory> { }
