// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MultiRegion
//  Layer:          Domain
//  File:           IRegionFailoverPolicy.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase 7 §4.3 (Global Routing) domain service deciding which region a real
//                  failover event would cut traffic over to, given a snapshot of every region's
//                  current health status. Real, deterministic, testable domain logic -- the
//                  actual network-level cutover (DNS/traffic-manager reconfiguration) is
//                  infrastructure this codebase cannot provision; see
//                  docs\Phase7\multi-region-dr-runbook.md for the concrete manual/automated
//                  steps a real failover event would require. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;

namespace PlantSafety.Domain.MultiRegion.Services;

/// <summary>
/// Decides which region traffic currently assigned to an unhealthy region should fail over to,
/// given a snapshot of every known region's current health status.
/// </summary>
public interface IRegionFailoverPolicy
{
    /// <summary>
    /// Given <paramref name="currentRegion"/>'s degraded/unavailable state and a snapshot of
    /// every known region's current health, deterministically decides the failover target
    /// region.
    /// </summary>
    /// <param name="currentRegion">The region currently experiencing a health problem.</param>
    /// <param name="regionHealth">
    /// A snapshot of every known region's current <see cref="RegionHealthStatus"/>. Must contain
    /// an entry for every region in <see cref="Region.All"/>.
    /// </param>
    /// <returns>
    /// The <see cref="Region"/> to fail over to, or <see langword="null"/> if no other region is
    /// currently healthy enough to receive failover traffic.
    /// </returns>
    Region? DetermineFailoverTarget(Region currentRegion, IReadOnlyDictionary<Region, RegionHealthStatus> regionHealth);
}
