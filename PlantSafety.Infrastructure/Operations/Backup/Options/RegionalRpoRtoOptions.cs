// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Operations.Backup
//  Layer:          Infrastructure
//  File:           RegionalRpoRtoOptions.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase 7 §4.4 (Disaster Recovery) sibling to RpoRtoOptions (Phase 6 §6.4).
//                  RpoRtoOptions formalizes RPO/RTO targets per data-tier ("Critical",
//                  "Standard") with NO region dimension -- it was written when this platform
//                  was understood as single-instance. This type adds region as an explicit
//                  second dimension, for the (currently hypothetical, since no real
//                  multi-region deployment exists) case where the acceptable RPO/RTO for a
//                  given tier legitimately differs by region -- e.g. a region with a real,
//                  provisioned geo-replica (see GeoReplicationOptions) can credibly target a
//                  tighter RPO than a region with none.
//
//                  This is a sibling, NOT a replacement: RpoRtoOptions/BackupRpoHealthCheck
//                  continue to be the real, currently-exercised check against the one real SQL
//                  Server instance in this environment. RegionalRpoRtoOptions is additive
//                  configuration scaffolding for the multi-region future; it is not yet wired
//                  to any health check because there is no per-region backup telemetry to
//                  compare it against (GeoReplicationOptions.IsReplicaProvisioned is false for
//                  every region today -- see that type's file header). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Operations.Backup.Options;

/// <summary>
/// A single region+tier combination's Recovery Point Objective / Recovery Time Objective
/// targets.
/// </summary>
/// <param name="RegionCode">The <see cref="PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion.Region"/> code this target applies to.</param>
/// <param name="Tier">The tier name (e.g. "Critical", "Standard"), matching <see cref="RpoRtoOptions"/>'s tier naming.</param>
/// <param name="RecoveryPointObjective">Maximum acceptable data-loss window for this region+tier combination.</param>
/// <param name="RecoveryTimeObjective">Maximum acceptable downtime for this region+tier combination.</param>
public sealed record RegionalRpoRtoTarget(
    string RegionCode,
    string Tier,
    TimeSpan RecoveryPointObjective,
    TimeSpan RecoveryTimeObjective);

/// <summary>
/// Strongly typed configuration for formalized, per-region RPO/RTO targets. Bound from
/// configuration section "Operations:Backup:RegionalRpoRto".
/// </summary>
public sealed class RegionalRpoRtoOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "Operations:Backup:RegionalRpoRto";

    /// <summary>
    /// The configured per-region+tier targets. Defaults mirror <see cref="RpoRtoOptions"/>'s
    /// two default tiers, applied uniformly to all four platform regions (since no region has a
    /// real, provisioned geo-replica in this environment today, there is no honest basis to
    /// differentiate targets by region yet -- operators should override these once real
    /// per-region replication infrastructure exists and its measured characteristics are known).
    /// </summary>
    public IReadOnlyList<RegionalRpoRtoTarget> Targets { get; init; } =
    [
        new("UsEast", "Critical", TimeSpan.FromHours(1), TimeSpan.FromHours(4)),
        new("UsEast", "Standard", TimeSpan.FromHours(24), TimeSpan.FromHours(24)),
        new("UsWest", "Critical", TimeSpan.FromHours(1), TimeSpan.FromHours(4)),
        new("UsWest", "Standard", TimeSpan.FromHours(24), TimeSpan.FromHours(24)),
        new("Eu", "Critical", TimeSpan.FromHours(1), TimeSpan.FromHours(4)),
        new("Eu", "Standard", TimeSpan.FromHours(24), TimeSpan.FromHours(24)),
        new("Apac", "Critical", TimeSpan.FromHours(1), TimeSpan.FromHours(4)),
        new("Apac", "Standard", TimeSpan.FromHours(24), TimeSpan.FromHours(24)),
    ];

    /// <summary>
    /// Resolves the <see cref="RegionalRpoRtoTarget"/> for <paramref name="regionCode"/> and
    /// <paramref name="tier"/>, or <see langword="null"/> if no matching target is configured.
    /// </summary>
    public RegionalRpoRtoTarget? ResolveTarget(string regionCode, string tier)
        => Targets.FirstOrDefault(t =>
            string.Equals(t.RegionCode, regionCode, StringComparison.OrdinalIgnoreCase)
            && string.Equals(t.Tier, tier, StringComparison.OrdinalIgnoreCase));
}
