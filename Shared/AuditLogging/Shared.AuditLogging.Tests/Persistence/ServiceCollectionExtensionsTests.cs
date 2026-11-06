// ============================================================================
//  Project:        Shared.AuditLogging
//  Module:         Tests.Persistence
//  Layer:          Tests (Unit)
//  File:           ServiceCollectionExtensionsTests.cs
//  Created:        2026-07-26
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for AddSharedAuditLogging DI registration and for
//                  AuditLoggingOptions defaults. NOTE: PlantSafety.Tests had no prior
//                  repository-level tests for AuditLogDapperRepository (only
//                  SearchAuditLogQueryHandlerTests, which exercised the MediatR handler
//                  against a substituted IAuditLogRepository) -- there was nothing to
//                  move for the repository itself. AuditLogDapperRepository opens a live
//                  SqlConnection and calls dbo.usp_SearchAuditLog, so its query behavior
//                  is exercised by IAuditLogRepository consumers' own tests (e.g.
//                  PlantSafety.Tests' SearchAuditLogQueryHandlerTests, substituting
//                  IAuditLogRepository) plus integration tests against a real
//                  PlantSafety.AuditDB, not unit tests here.
// ============================================================================
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Shared.AuditLogging.Abstractions.Repositories;
using Shared.AuditLogging.Infrastructure.DependencyInjection;
using Shared.AuditLogging.Infrastructure.Options;
using Shared.AuditLogging.Infrastructure.Persistence;

using Xunit;

namespace Shared.AuditLogging.Tests.Persistence;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddSharedAuditLogging_RegistersAuditLogRepository()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();

        services.AddSharedAuditLogging(configuration);
        using var provider = services.BuildServiceProvider();

        var repository = provider.GetRequiredService<IAuditLogRepository>();
        Assert.IsType<AuditLogDapperRepository>(repository);
    }

    [Fact]
    public void AddSharedAuditLogging_BindsOptionsFromConfiguredSection()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Shared:AuditLogging:AuditDbConnectionString"] = "Server=test;Database=PlantSafety.AuditDB;",
                ["Shared:AuditLogging:SearchCommandTimeoutSeconds"] = "30",
                ["Shared:AuditLogging:MaxPageSize"] = "1000",
            })
            .Build();

        services.AddSharedAuditLogging(configuration);
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<AuditLoggingOptions>>().Value;

        Assert.Equal("Server=test;Database=PlantSafety.AuditDB;", options.AuditDbConnectionString);
        Assert.Equal(30, options.SearchCommandTimeoutSeconds);
        Assert.Equal(1000, options.MaxPageSize);
    }

    [Fact]
    public void AuditLoggingOptions_HasDeterministicDefaults()
    {
        var options = new AuditLoggingOptions();

        Assert.Contains("PlantSafety.AuditDB", options.AuditDbConnectionString);
        Assert.Equal(60, options.SearchCommandTimeoutSeconds);
        Assert.Equal(5000, options.MaxPageSize);
    }
}
