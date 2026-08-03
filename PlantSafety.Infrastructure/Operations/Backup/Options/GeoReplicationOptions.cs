// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Operations.Backup
//  Layer:          Infrastructure
//  File:           GeoReplicationOptions.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase 7 §4.2 (Geo-Replication) configuration sibling to RpoRtoOptions
//                  (Phase 6 §6.4). RpoRtoOptions formalizes backup-recency targets for the
//                  single, real SQL Server instance that exists in this environment;
//                  GeoReplicationOptions formalizes the *per-region replication-lag target*
//                  that WOULD apply once a real secondary replica exists in another region.
//
//                  HONEST LIMITATION: no real geo-replica exists anywhere in this sandboxed dev
//                  environment (there is exactly one SQL Server instance). This type is real,
//                  correct configuration scaffolding -- bound from configuration, validated,
//                  consumed by GeoReplicationHealthCheck -- but GeoReplicationHealthCheck cannot
//                  query real replication-lag telemetry (e.g. sys.dm_hadr_database_replica_states)
//                  because there is no secondary to query. See GeoReplicationHealthCheck's own
//                  file header for exactly how it reports this honestly rather than fabricating a
//                  "healthy" status. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Operations.Backup.Options;

/// <summary>
/// A single region's configured (not necessarily provisioned) replication-lag target.
/// </summary>
/// <param name="RegionCode">The <see cref="PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion.Region"/> code this target applies to.</param>
/// <param name="IsReplicaProvisioned">
/// Whether a real secondary replica is actually provisioned for this region in the current
/// environment. Defaults to <see langword="false"/> for every region in this sandbox -- there is
/// no real secondary anywhere. Operators must explicitly flip this to <see langword="true"/>
/// only once a real replica exists and is queryable, so <see cref="GeoReplicationHealthCheck"/>
/// never silently reports a fabricated healthy status for a region with no real replica.
/// </param>
/// <param name="MaxAcceptableReplicationLag">
/// Maximum acceptable replication lag before the region's replication is considered degraded,
/// once a real replica is provisioned and queryable.
/// </param>
public sealed record GeoReplicationTarget(
    string RegionCode,
    bool IsReplicaProvisioned,
    TimeSpan MaxAcceptableReplicationLag);

/// <summary>
/// Strongly typed configuration for per-region geo-replication targets. Bound from configuration
/// section "Operations:Backup:GeoReplication".
/// </summary>
public sealed class GeoReplicationOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "Operations:Backup:GeoReplication";

    /// <summary>
    /// The configured per-region replication targets. Defaults to all four platform regions
    /// (see <see cref="PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion.Region"/>) with
    /// <see cref="GeoReplicationTarget.IsReplicaProvisioned"/> = <see langword="false"/> for every
    /// one -- the honest default for this sandbox, where no real secondary replica exists
    /// anywhere. Operators overriding this via configuration must only set
    /// <c>IsReplicaProvisioned: true</c> for a region once a real, queryable secondary exists.
    /// </summary>
    public IReadOnlyList<GeoReplicationTarget> Targets { get; init; } =
    [
        new("UsEast", false, TimeSpan.FromMinutes(5)),
        new("UsWest", false, TimeSpan.FromMinutes(5)),
        new("Eu", false, TimeSpan.FromMinutes(5)),
        new("Apac", false, TimeSpan.FromMinutes(10)),
    ];

    /// <summary>Resolves the <see cref="GeoReplicationTarget"/> for <paramref name="regionCode"/>, or <see langword="null"/> if not configured.</summary>
    public GeoReplicationTarget? ResolveTarget(string regionCode)
        => Targets.FirstOrDefault(t => string.Equals(t.RegionCode, regionCode, StringComparison.OrdinalIgnoreCase));
}
