// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Tests.Persistence
//  Layer:          Tests (Unit)
//  File:           ServiceCollectionExtensionsTests.cs
//  Created:        2026-07-27
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for AddSharedSafetyData DI registration and for
//                  SafetyDataOptions defaults. Mirrors Shared.AuditLogging.Tests'
//                  ServiceCollectionExtensionsTests.
//
//                  Phase 4.2: SafetyDataDapperRepository now goes through
//                  Shared.SqlGateway's ISqlGateway instead of opening its own
//                  SqlConnection, so AddSharedSafetyData now also requires
//                  Shared.Deterministic's IClock and Shared.Telemetry's
//                  ILogEventWriter/ICorrelationContext to be registered (the
//                  same prerequisite Shared.SqlGateway.Tests' own
//                  ServiceCollectionExtensionsTests already established for
//                  AddSqlGateway directly) -- resolving ISafetyDataRepository
//                  itself stays a pure DI-graph check, no live SQL Server
//                  needed; real query behavior is exercised by
//                  ISafetyDataRepository consumers' own tests (once Reporting
//                  wires up a consumer) plus integration tests against a real
//                  PlantSafety.CoreDB, not unit tests here.
// ============================================================================
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Shared.Deterministic.Abstractions.Clock;
using Shared.SafetyData.Abstractions.Repositories;
using Shared.SafetyData.Infrastructure.DependencyInjection;
using Shared.SafetyData.Infrastructure.Options;
using Shared.SafetyData.Infrastructure.Persistence;
using Shared.Telemetry.Abstractions.Correlation;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.SafetyData.Tests.Persistence;

public sealed class ServiceCollectionExtensionsTests
{
    private sealed class NoOpLogEventWriter : ILogEventWriter
    {
        public void Write(ILogEvent logEvent)
        {
        }
    }

    private sealed class FixedCorrelationContext : ICorrelationContext
    {
        public string CorrelationId => "test-correlation";
    }

    private static IServiceCollection BuildBaseServices(Dictionary<string, string?> configValues, out IConfiguration configuration)
    {
        configuration = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(configuration);
        services.AddSingleton<IClock>(new FixedClock(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        services.AddSingleton<ILogEventWriter, NoOpLogEventWriter>();
        services.AddSingleton<ICorrelationContext, FixedCorrelationContext>();
        return services;
    }

    [Fact]
    public void AddSharedSafetyData_RegistersSafetyDataRepository()
    {
        IServiceCollection services = BuildBaseServices(
            new Dictionary<string, string?> { ["Shared:SqlGateway:ConnectionString"] = "Server=test;Database=PlantSafety.CoreDB;" },
            out IConfiguration configuration);

        services.AddSharedSafetyData(configuration);
        using var provider = services.BuildServiceProvider();

        var repository = provider.GetRequiredService<ISafetyDataRepository>();
        Assert.IsType<SafetyDataDapperRepository>(repository);
    }

    [Fact]
    public void AddSharedSafetyData_BindsOptionsFromConfiguredSection()
    {
        IServiceCollection services = BuildBaseServices(
            new Dictionary<string, string?>
            {
                ["Shared:SqlGateway:ConnectionString"] = "Server=test;Database=PlantSafety.CoreDB;",
                ["Shared:SafetyData:MaxPageSize"] = "1000",
            },
            out IConfiguration configuration);

        services.AddSharedSafetyData(configuration);
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<SafetyDataOptions>>().Value;

        Assert.Equal(1000, options.MaxPageSize);
    }

    [Fact]
    public void SafetyDataOptions_HasDeterministicDefaults()
    {
        var options = new SafetyDataOptions();

        Assert.Equal(5000, options.MaxPageSize);
    }
}
