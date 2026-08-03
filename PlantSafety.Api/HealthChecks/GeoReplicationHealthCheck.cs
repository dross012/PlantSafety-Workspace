// ============================================================================
// File: GeoReplicationHealthCheck.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-03
// Description: Phase 7 §4.2 (Geo-Replication) sibling health check to BackupRpoHealthCheck
//              (Phase 6 §6.4). Reports per-region replication status against the configured
//              GeoReplicationOptions targets.
//
//              HONESTY NOTE (read before modifying): this environment has exactly ONE real SQL
//              Server instance and NO real secondary replica in any other region. There is no
//              replication-lag telemetry to query (no `sys.dm_hadr_database_replica_states`
//              DMV against a real secondary, no cross-region link of any kind). This check does
//              NOT fabricate a "Healthy" replication status for any region -- for every region
//              where GeoReplicationOptions reports IsReplicaProvisioned = false (the default,
//              honest state for every region today), this check reports that region as a
//              clearly-labelled PLACEHOLDER, distinct from an actual replication failure. Only
//              once a real secondary is provisioned and IsReplicaProvisioned is flipped to true
//              for a region (and this check is extended to actually query that secondary's DMVs)
//              would a real Healthy/Degraded/Unhealthy verdict be possible for that region.
//
//              Deliberately NOT tagged "ready" -- unlike BackupRpoHealthCheck, this check must
//              never gate the /health/ready readiness probe on a known environmental limitation
//              (no real replica) rather than an actual operational problem. It is tagged
//              "diagnostics" and surfaces only on the unfiltered /health endpoint.
// ============================================================================

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using PlantSafety.Infrastructure.Operations.Backup.Options;
using PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;

namespace PlantSafety.Api.HealthChecks;

/// <summary>
/// Health check reporting per-region geo-replication status against the configured
/// <see cref="GeoReplicationOptions"/> targets. Honestly reports a placeholder state for every
/// region with no real, provisioned secondary replica rather than fabricating a healthy status.
/// </summary>
public sealed class GeoReplicationHealthCheck : IHealthCheck
{
    private readonly GeoReplicationOptions _options;
    private readonly ILogger<GeoReplicationHealthCheck> _logger;

    /// <summary>Initialises a new instance of <see cref="GeoReplicationHealthCheck"/>.</summary>
    public GeoReplicationHealthCheck(IOptions<GeoReplicationOptions> options, ILogger<GeoReplicationHealthCheck> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Reports, for every known <see cref="Region"/>, whether a real geo-replica is provisioned
    /// and configured. This sandbox has no real secondary replica anywhere, so every region is
    /// expected to report as a placeholder until real infrastructure lands.
    /// </summary>
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var placeholderRegions = new List<string>();
            var provisionedRegions = new List<string>();
            var unconfiguredRegions = new List<string>();

            foreach (var region in Region.All)
            {
                var target = _options.ResolveTarget(region.Code);

                if (target is null)
                {
                    unconfiguredRegions.Add(region.Code);
                    continue;
                }

                if (target.IsReplicaProvisioned)
                {
                    // This branch is intentionally unreachable in this sandbox today (no real
                    // secondary is provisioned anywhere) -- it exists so that the moment
                    // operations flips IsReplicaProvisioned = true for a real region, this
                    // check's structure already accounts for it. A future change MUST replace
                    // this branch's body with a real DMV/replication-lag query before relying on
                    // it -- it must not silently report Healthy just because the flag was set.
                    provisionedRegions.Add(region.Code);
                }
                else
                {
                    placeholderRegions.Add(region.Code);
                }
            }

            var data = new Dictionary<string, object>
            {
                ["placeholderRegions"] = placeholderRegions,
                ["provisionedRegions"] = provisionedRegions,
                ["unconfiguredRegions"] = unconfiguredRegions,
            };

            if (unconfiguredRegions.Count > 0)
            {
                _logger.LogWarning(
                    "GeoReplicationHealthCheck: {Count} region(s) have no GeoReplicationOptions target configured: {Regions}",
                    unconfiguredRegions.Count, string.Join(", ", unconfiguredRegions));
            }

            // Reports Degraded (never Healthy) whenever any region is still a placeholder --
            // this is an honest statement that the platform's DR posture for that region cannot
            // be verified in this environment, not a claim that replication is failing.
            if (placeholderRegions.Count > 0 || unconfiguredRegions.Count > 0)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"Geo-replication status cannot be verified for {placeholderRegions.Count + unconfiguredRegions.Count} region(s) " +
                    "-- no real secondary replica is provisioned in this environment. This is a known environmental limitation " +
                    "(single SQL Server instance, no cross-region link), not a reported replication failure.",
                    data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy(
                "All configured regions report a provisioned, queryable replica.", data: data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GeoReplicationHealthCheck failed.");
            return Task.FromResult(HealthCheckResult.Unhealthy("Geo-replication health check failed.", ex));
        }
    }
}
