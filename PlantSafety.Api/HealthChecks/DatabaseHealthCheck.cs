// ============================================================================
// File: DatabaseHealthCheck.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-12
// Last Updated: 2026-06-12
// Description: ASP.NET Core health check that verifies Azure SQL Server connectivity
//              for the /health/ready readiness probe.
// ============================================================================
//
// Onboarding Notes:
// - Registered in Extensions/ServiceCollectionExtensions.cs and mapped to the
//   /health/ready endpoint in Program.cs (readiness probe only, not liveness).
// - Uses Dapper for the health check query to stay consistent with the platform's
//   no-EF-Core, Dapper-only data access policy.
// - The connection string is retrieved from Azure Key Vault via IConfiguration.
//   Connection strings are NEVER stored in appsettings.json.
// - This health check tests the primary Azure SQL Server connection. Individual module
//   schema health is NOT validated here — that is out of scope for a liveness/readiness check.
// - See Architecture Specification §4.10 (Health Checks) and §8.5 (Observability).
//
// Rationale:
// - Kubernetes readiness probes require a lightweight, reliable database connectivity check
//   so that traffic is not routed to pods that cannot reach their data store.
//
// Future Extension Seams:
// - Add per-module schema connectivity checks as separate named health checks.
// - Add query latency measurement to detect degraded (not just failed) database connections.
//
// ============================================================================

using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PlantSafety.Api.HealthChecks;

/// <summary>
/// Health check that validates Azure SQL Server connectivity by executing a lightweight
/// SELECT 1 query. Used by the /health/ready readiness probe.
/// </summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
    private const string ConnectionStringKey = "ConnectionStrings:PlantSafetyDb";
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="DatabaseHealthCheck"/>.
    /// </summary>
    /// <param name="configuration">Configuration provider backed by Azure Key Vault in production.</param>
    /// <param name="logger">Logger for recording health check failures.</param>
    public DatabaseHealthCheck(IConfiguration configuration, ILogger<DatabaseHealthCheck> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Executes a lightweight SQL connectivity check against the Azure SQL Server instance.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// <see cref="HealthCheckResult.Healthy"/> if the database responds within the timeout;
    /// <see cref="HealthCheckResult.Unhealthy"/> otherwise.
    /// </returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionString = _configuration[ConnectionStringKey];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return HealthCheckResult.Unhealthy("Database connection string is not configured.");
            }

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await connection.QuerySingleAsync<int>("SELECT 1;");

            return HealthCheckResult.Healthy("Database connection is healthy.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed.");
            return HealthCheckResult.Unhealthy("Database connection failed.", ex);
        }
    }
}
