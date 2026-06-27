// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Common
//  Layer:          IntegrationTests
//  File:           IntegrationTestBase.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Abstract base class for all integration tests providing shared factory, client, and assertion helpers.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
namespace PlantSafety.IntegrationTests.Common.TestFixtures;

/// <summary>
/// Abstract base for all integration tests.  Provides the shared
/// <see cref="IntegrationTestWebAppFactory"/>, a pre-configured
/// <see cref="HttpClient"/>, and access to the SQL connection string.
/// </summary>
public abstract class IntegrationTestBase(IntegrationTestWebAppFactory factory)
{
    /// <summary>Shared test web application factory.</summary>
    protected IntegrationTestWebAppFactory Factory { get; } = factory;

    /// <summary>Pre-configured HttpClient for API requests.</summary>
    protected HttpClient Client { get; } = factory.CreateClient();

    /// <summary>Testcontainers SQL Server connection string.</summary>
    protected string ConnectionString => factory.ConnectionString;

    /// <summary>Canonical test tenant identifier.</summary>
    protected static Guid TenantId => TestIds.TenantId;

    /// <summary>Canonical test actor identifier.</summary>
    protected static Guid ActorId => TestIds.ActorId;

    /// <summary>Canonical test correlation identifier.</summary>
    protected static Guid CorrelationId => TestIds.CorrelationId;

    /// <summary>Fixed UTC timestamp for deterministic test assertions.</summary>
    protected static DateTime FixedUtcNow => TestClock.FixedUtcNow;
}
