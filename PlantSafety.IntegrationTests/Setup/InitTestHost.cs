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
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PlantSafety.Infrastructure.Common.Configuration;
using Shared.AuditLogging.Infrastructure.Options;

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
        await TestDatabaseFactory.ApplySharedAuditLoggingSchemaAsync(ConnectionString);
        await TestDatabaseFactory.ApplyIncidentManagementSchemaAsync(ConnectionString);
        await TestDatabaseFactory.ApplyRiskManagementSchemaAsync(ConnectionString);
        await TestDatabaseFactory.ApplyAccidentManagementSchemaAsync(ConnectionString);
        await TestDatabaseFactory.ApplyComplianceSchemaAsync(ConnectionString);
        await TestDatabaseFactory.ApplyAnalyticsSchemaAsync(ConnectionString);
        await TestDatabaseFactory.ApplyOperationalSchemaAsync(ConnectionString);
        await TestDatabaseFactory.ApplyFinancialsSchemaAsync(ConnectionString);
        await TestDatabaseFactory.ApplyPlanningProjectsSchemaAsync(ConnectionString);
        await TestDatabaseFactory.ApplyContinuousImprovementSchemaAsync(ConnectionString);
        await TestDatabaseFactory.ApplyIntelligenceSchemaAsync(ConnectionString);
        await TestDatabaseFactory.ApplyIntegrationsSchemaAsync(ConnectionString);
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

                // Shared.AuditLogging's AuditLogDapperRepository opens its own SqlConnection
                // against AuditLoggingOptions.AuditDbConnectionString rather than the tenant
                // DatabaseOptions connection strings above (dbo.AuditLog is cross-database,
                // server-level platform data -- see AuditLoggingOptions.cs). Point it at the
                // same Testcontainers instance so SearchAuditLogQueryHandler round-trips
                // through the real Shared.AuditLogging stack in integration tests.
                [$"{AuditLoggingOptions.SectionName}:{nameof(AuditLoggingOptions.AuditDbConnectionString)}"] = ConnectionString,
            });
        });

        // CreateCultureCommandHandler now depends only on ICultureRepository (implemented by
        // PlantSafety.Infrastructure.Analytics.Culture.Persistence.CultureRepository and
        // registered via AnalyticsLegacyRepositoryRegistration.AddAnalyticsLegacyRepositories,
        // called from InfrastructureServiceRegistration). No test-only DI stubs are required here
        // anymore -- see CreateCultureCommandHandler's file header for why the previously-required
        // SharedKernel.Abstractions.IUnitOfWork / Application-layer IOutboxWriter stubs were removed.

        // InfrastructureServiceRegistration.AddInfrastructure registers IDistributedCache via
        // AddStackExchangeRedisCache(opts => opts.Configuration = configuration.GetConnectionString("Redis")),
        // which this test host never supplies -- every MediatR request goes through
        // CachingBehavior<,>, which calls ApplicationCacheServiceAdapter.GetAsync, which throws
        // ArgumentNullException from StackExchange.Redis's ConfigurationOptions.Parse(null) the
        // moment any command/query is sent, regardless of module. No Testcontainers Redis instance
        // is provisioned (unlike SQL Server) -- swap in an in-memory IDistributedCache for the test
        // host instead, the standard WebApplicationFactory pattern for out-of-process test
        // dependencies that don't need to be exercised for real.
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDistributedCache>();
            services.AddDistributedMemoryCache();
        });
    }
}

/// <summary>xunit v3 collection fixture — shares the test host across all tests in the assembly.</summary>
[CollectionDefinition(nameof(IntegrationTestCollection))]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebAppFactory> { }
