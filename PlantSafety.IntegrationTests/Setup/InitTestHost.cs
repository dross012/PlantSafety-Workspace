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
using PlantSafety.Application.Abstractions.Security;
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
        await TestDatabaseFactory.ApplyCrossModuleDedupSchemaAsync(ConnectionString);
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

        // Phase 7 §5.4 Audit & Logging (Security & Compliance Agent, 2026-08-03): backs
        // AuthorizationBehavior's PermissionUsageLog capture and GetTenantAuditStreamQuery.
        await TestDatabaseFactory.ApplyComplianceMonitoringSchemaAsync(ConnectionString);

        // HazardousWasteManagement module (Testing Agent, 2026-08-04): backs the WasteStreams,
        // WasteContainers, AccumulationAreas, Manifests, Transporters, TsdfFacilities, and
        // DisposalCertificates Dapper repositories for full command/query round-trip tests.
        await TestDatabaseFactory.ApplyHazardousWasteManagementSchemaAsync(ConnectionString);

        // AutonomousVehicleSafety module (Testing Agent, 2026-08-04): backs the
        // AutonomousVehicleAsset, AutonomousVehicleIncident, HumanRobotInteractionEvent,
        // OperatingGeofence, and SafetyCaseValidation Dapper repositories for full
        // command/query round-trip and API integration tests.
        await TestDatabaseFactory.ApplyAutonomousVehicleSafetySchemaAsync(ConnectionString);

        // CyberPhysicalSecurity module (Testing Agent, 2026-08-04): backs the OtAsset,
        // ControlChangeEvent (tamper-evident SHA-256 hash chain), SensorIntegrityCheck,
        // SabotageIndicator, and CyberPhysicalIncident Dapper repositories for full
        // command/query round-trip and API integration tests.
        await TestDatabaseFactory.ApplyCyberPhysicalSecuritySchemaAsync(ConnectionString);

        // EnvironmentalHealth.WaterDischargeCompliance module (Testing Agent, 2026-08-04): backs
        // the DischargePermit, DischargePoint, EffluentSample, DischargeLimit, LimitExceedance,
        // and DischargeMonitoringReport Dapper repositories for full command/query round-trip and
        // API integration tests.
        await TestDatabaseFactory.ApplyWaterDischargeComplianceSchemaAsync(ConnectionString);

        // LogisticsTransportSafety module (Testing Agent, 2026-08-04): backs the
        // TransportVehicle, Carrier, DriverQualification, TransportTrip, LoadSecurementCheck,
        // and TransportIncident Dapper repositories for full command/query round-trip and API
        // integration tests.
        await TestDatabaseFactory.ApplyLogisticsTransportSafetySchemaAsync(ConnectionString);

        // CommunitySafety module (Testing Agent, 2026-08-04): backs the CommunityContact,
        // CommunityConcern, CommunityImpactAssessment, CommunityNotification, and
        // CommunityMeetingRecord Dapper repositories for full command/query round-trip and API
        // integration tests.
        await TestDatabaseFactory.ApplyCommunitySafetySchemaAsync(ConnectionString);

        // PhysicalAccessControl module (Testing Agent, 2026-08-04): backs the AccessPoint,
        // Credential, AccessEvent, SafetyZone, AccessAnomaly, and RestrictedZonePolicy Dapper
        // repositories for full command/query round-trip integration tests.
        await TestDatabaseFactory.ApplyPhysicalAccessControlSchemaAsync(ConnectionString);

        // PublicSafetyCommunication module (Testing Agent, 2026-08-04): backs the
        // PublicSafetyAgency (+ AgencyContact), IncidentNotification, PublicAlertBroadcast, and
        // append-only CommunicationLogEntry Dapper repositories for full command/query
        // round-trip and API integration tests.
        await TestDatabaseFactory.ApplyPublicSafetyCommunicationSchemaAsync(ConnectionString);

        // MachineSafety.CraneHoistSystems sub-aggregate (Testing Agent, 2026-08-04): backs
        // CraneHoistSystemsDapperRepository (asset registry, load-limit enforcement, inspection
        // cycle, operator qualification, rigging safety) for full command/query round-trip and
        // API integration tests. Scoped only to CraneHoistSystems -- the other 6 MachineSafety
        // sub-aggregates are out of scope here.
        await TestDatabaseFactory.ApplyMachineSafetyCraneHoistSystemsSchemaAsync(ConnectionString);

        // HumanFactors.CognitiveLoadAnalytics sub-module (Testing Agent, 2026-08-04): backs
        // CognitiveLoadLedgerDapperRepository, CognitiveRiskPredictionDapperRepository, and
        // CognitiveLoadAnalyticsReadDapperRepository for full command/query round-trip and API
        // integration tests. Scoped only to CognitiveLoadAnalytics -- the base HumanFactors
        // aggregate is out of scope here.
        await TestDatabaseFactory.ApplyHumanFactorsCognitiveLoadAnalyticsSchemaAsync(ConnectionString);

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

            // NOTE on the tenant-context fix: an earlier version of this change added a blanket
            // authentication-scheme override here (every Client request auto-authenticated as the
            // TestIds identity). Reverted -- it broke ~122 "...WithoutAuth_ShouldReturn401" /
            // "...InvalidTenant..." tests that deliberately send unauthenticated requests through
            // the same shared Client to verify [Authorize] actually rejects them, since there was
            // no way to distinguish "wants real auth" from "wants to test the 401 path" on the
            // same client instance. The dominant root cause (~125 of ~135 failures) was
            // ServiceProviderExtensions.GetScopedService -- see its header comment -- which needed
            // no HTTP auth scheme at all, since it never goes through HTTP. If a genuine future
            // need arises for an authenticated Client in a specific test, prefer a
            // second/dedicated HttpClient with an explicit test bearer token over re-introducing a
            // blanket override here.

            // See AllowAllTestPermissionService's header comment: these tests verify handler-to-
            // SQL round-trips, not fine-grained permission-string enforcement.
            services.RemoveAll<IPermissionService>();
            services.AddScoped<IPermissionService, AllowAllTestPermissionService>();
        });
    }
}

/// <summary>xunit v3 collection fixture — shares the test host across all tests in the assembly.</summary>
[CollectionDefinition(nameof(IntegrationTestCollection))]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebAppFactory> { }
