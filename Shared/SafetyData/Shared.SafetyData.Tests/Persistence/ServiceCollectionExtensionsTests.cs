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
//                  ServiceCollectionExtensionsTests. SafetyDataDapperRepository
//                  opens a live SqlConnection and calls stored procedures against
//                  PlantSafety.CoreDB, so its query behavior is exercised by
//                  ISafetyDataRepository consumers' own tests (once Reporting
//                  wires up a consumer) plus integration tests against a real
//                  PlantSafety.CoreDB, not unit tests here.
// ============================================================================
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Shared.SafetyData.Abstractions.Repositories;
using Shared.SafetyData.Infrastructure.DependencyInjection;
using Shared.SafetyData.Infrastructure.Options;
using Shared.SafetyData.Infrastructure.Persistence;

using Xunit;

namespace Shared.SafetyData.Tests.Persistence;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddSharedSafetyData_RegistersSafetyDataRepository()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();

        services.AddSharedSafetyData(configuration);
        using var provider = services.BuildServiceProvider();

        var repository = provider.GetRequiredService<ISafetyDataRepository>();
        Assert.IsType<SafetyDataDapperRepository>(repository);
    }

    [Fact]
    public void AddSharedSafetyData_BindsOptionsFromConfiguredSection()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Shared:SafetyData:CoreDbConnectionString"] = "Server=test;Database=PlantSafety.CoreDB;",
                ["Shared:SafetyData:QueryCommandTimeoutSeconds"] = "30",
                ["Shared:SafetyData:MaxPageSize"] = "1000",
            })
            .Build();

        services.AddSharedSafetyData(configuration);
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<SafetyDataOptions>>().Value;

        Assert.Equal("Server=test;Database=PlantSafety.CoreDB;", options.CoreDbConnectionString);
        Assert.Equal(30, options.QueryCommandTimeoutSeconds);
        Assert.Equal(1000, options.MaxPageSize);
    }

    [Fact]
    public void SafetyDataOptions_HasDeterministicDefaults()
    {
        var options = new SafetyDataOptions();

        Assert.Contains("PlantSafety.CoreDB", options.CoreDbConnectionString);
        Assert.Equal(60, options.QueryCommandTimeoutSeconds);
        Assert.Equal(5000, options.MaxPageSize);
    }
}
