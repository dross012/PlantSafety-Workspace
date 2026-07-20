// ============================================================================
// File: SchemaVersionHealthCheck.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-20
// Description: ASP.NET Core health check that reports schema drift for the /health/ready
//              readiness probe by comparing the Migrations\ manifest against each live
//              database's [System].[SchemaMigrations] table.
// ============================================================================
//
// Onboarding Notes:
// - Registered in Extensions/ServiceCollectionExtensions.cs alongside DatabaseHealthCheck/
//   RedisHealthCheck/ServiceBusHealthCheck, mapped to /health/ready.
// - Deliberately does NOT hard-block process startup on drift: this API instance can still
//   serve traffic for databases that ARE current, and a hard startup gate would make local
//   dev impossible whenever a migration script is legitimately pending review. Reporting
//   Unhealthy on the readiness probe (rather than crashing the process) lets orchestration
//   correctly stop routing new traffic to a drifted instance while still allowing an
//   operator to reach diagnostics/admin endpoints to investigate.
// - Per-database detail is available via GetSchemaHealthQuery / GET
//   /api/v1/operations/schema-guard/health; this health check surfaces only a rolled-up
//   Healthy/Degraded/Unhealthy summary, matching the shape of the other /health/ready checks.
//
// ============================================================================

using System.Linq;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PlantSafety.Application.Operations.SchemaGuard.Queries;

namespace PlantSafety.Api.HealthChecks;

/// <summary>
/// Health check that reports schema drift across all known PlantSafety.* databases.
/// Used by the /health/ready readiness probe.
/// </summary>
public sealed class SchemaVersionHealthCheck : IHealthCheck
{
    private readonly ISender _sender;
    private readonly ILogger<SchemaVersionHealthCheck> _logger;

    /// <summary>Initializes a new instance of <see cref="SchemaVersionHealthCheck"/>.</summary>
    public SchemaVersionHealthCheck(ISender sender, ILogger<SchemaVersionHealthCheck> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Evaluates schema drift for every known database via <see cref="GetSchemaHealthQuery"/>.
    /// </summary>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _sender.Send(new GetSchemaHealthQuery(), cancellationToken);

            if (result.IsFailure)
            {
                return HealthCheckResult.Unhealthy($"Schema health could not be evaluated: {result.Error.Message}");
            }

            var reports = result.Value;
            var drifted = reports.Where(r => r.Status == "Drifted").ToList();
            var unknown = reports.Where(r => r.Status == "Unknown").ToList();

            if (drifted.Count > 0)
            {
                var summary = string.Join("; ", drifted.Select(d => $"{d.DatabaseKey}: {d.MissingScripts.Count} missing"));
                return HealthCheckResult.Unhealthy($"Schema drift detected on {drifted.Count} database(s): {summary}");
            }

            if (unknown.Count > 0)
            {
                var summary = string.Join(", ", unknown.Select(u => u.DatabaseKey));
                return HealthCheckResult.Degraded($"Schema state could not be verified for: {summary}");
            }

            return HealthCheckResult.Healthy($"Schema is current on all {reports.Count} known database(s).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Schema version health check failed.");
            return HealthCheckResult.Unhealthy("Schema version check failed.", ex);
        }
    }
}
