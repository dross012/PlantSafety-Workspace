// ============================================================================
// File: DatabaseHealthCheck.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-12
// Last Updated: 2026-08-21
// Description: ASP.NET Core health check that verifies SQL Server connectivity
//              for the /health/ready readiness probe.
// ============================================================================
//
// Onboarding Notes:
// - Registered in Extensions/ServiceCollectionExtensions.cs and mapped to the
//   /health/ready endpoint in Program.cs (readiness probe only, not liveness).
// - Uses Dapper for the health check query to stay consistent with the platform's
//   no-EF-Core, Dapper-only data access policy.
// - FIXED 2026-08-21: this check originally read `ConnectionStrings:PlantSafetyDb`
//   (an Azure Key Vault-era key this codebase never actually populates anywhere) and
//   so always reported Unhealthy regardless of real database state -- verified live
//   against a working PlantSafety.Api instance whose schema-version health check and
//   OutboxProcessor both proved real DB connectivity while this check failed with
//   "connection string is not configured." The real connection string this codebase's
//   actual repositories use lives at `Database:ConnectionString` (see
//   PlantSafety.Api/Appsettings.json and Group A of
//   source/docs/plantsafety-cutover-plan.md) -- this check now reads that key instead.
//   This is on-prem SQL Server (Express in dev, Standard in production), not Azure SQL
//   -- the "Azure Key Vault" premise in this file's original comments never reflected
//   this program's actual deployment target.
// - This health check tests the primary SQL Server connection. Individual module
//   schema health is NOT validated here -- see the separate schema-version health check
//   for that signal.
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
/// Health check that validates SQL Server connectivity by executing a lightweight
/// SELECT 1 query. Used by the /health/ready readiness probe.
/// </summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
    private const string ConnectionStringKey = "Database:ConnectionString";
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="DatabaseHealthCheck"/>.
    /// </summary>
    /// <param name="configuration">Configuration provider for the application's connection strings.</param>
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
