// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MultiRegion
//  Layer:          Domain
//  File:           RegionFailoverPolicy.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase 7 §4.3/§4.4 default IRegionFailoverPolicy implementation. Uses a fixed,
//                  documented pairing of each region to its designated DR partner (the region
//                  it fails over to first, when healthy):
//                    UsEast <-> UsWest   (both US data-residency zone -- no data-sovereignty
//                                         boundary crossed by failing over between them)
//                    Eu     <-> Apac     (deliberately NOT paired with a US region -- an EU
//                                         tenant's failover target must not cross the EU data-
//                                         residency boundary to a US region; Apac is the least-
//                                         US-entangled remaining option in this 4-region model.
//                                         See docs\Phase7\multi-region-architecture.md for why
//                                         US/EU/APAC data residency is treated as a hard
//                                         constraint here, not merely a preference.)
//                  If the designated partner is not Healthy, falls back to the first Healthy
//                  region (by canonical Region.Id order, excluding the current region) so the
//                  decision remains fully deterministic even in a multi-region-outage scenario.
//                  Returns null only when no other region is Healthy.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;

namespace PlantSafety.Domain.MultiRegion.Services;

/// <summary>
/// Default <see cref="IRegionFailoverPolicy"/> implementation: a fixed, data-residency-aware
/// primary DR pairing per region, falling back to the first healthy region in canonical order.
/// </summary>
public sealed class RegionFailoverPolicy : IRegionFailoverPolicy
{
    private static readonly IReadOnlyDictionary<Region, Region> PrimaryFailoverPartner =
        new Dictionary<Region, Region>
        {
            [Region.UsEast] = Region.UsWest,
            [Region.UsWest] = Region.UsEast,
            [Region.Eu] = Region.Apac,
            [Region.Apac] = Region.Eu,
        };

    /// <inheritdoc/>
    public Region? DetermineFailoverTarget(Region currentRegion, IReadOnlyDictionary<Region, RegionHealthStatus> regionHealth)
    {
        ArgumentNullException.ThrowIfNull(currentRegion);
        ArgumentNullException.ThrowIfNull(regionHealth);

        foreach (var region in Region.All)
        {
            if (!regionHealth.ContainsKey(region))
            {
                throw new ArgumentException(
                    $"regionHealth must contain an entry for every region in Region.All; missing '{region.Code}'.",
                    nameof(regionHealth));
            }
        }

        if (PrimaryFailoverPartner.TryGetValue(currentRegion, out var partner)
            && regionHealth[partner] == RegionHealthStatus.Healthy)
        {
            return partner;
        }

        // Primary partner is not healthy (or currentRegion is not a recognised region) -- fall
        // back to the first healthy region other than currentRegion, in canonical Id order, so
        // the decision stays fully deterministic.
        foreach (var candidate in Region.All)
        {
            if (candidate == currentRegion)
            {
                continue;
            }

            if (regionHealth[candidate] == RegionHealthStatus.Healthy)
            {
                return candidate;
            }
        }

        // No other region is healthy -- an honest "no failover target available" result rather
        // than picking a degraded/unavailable region and pretending it is a safe failover.
        return null;
    }
}
