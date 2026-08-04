// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
//  Layer:          Domain
//  File:           EvacuationRouteArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for EvacuationRoute aggregates.
//                  Compliance hook: retired routes feed NFPA 101 egress-mapping documentation and
//                  must remain queryable, not hard-deleted. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Policies;

/// <summary>Determines whether an <see cref="Aggregates.EvacuationRoute"/> aggregate is eligible for archival.</summary>
public static class EvacuationRouteArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the route record may be archived (retired only).</summary>
    public static bool IsEligibleForArchival(Aggregates.EvacuationRoute aggregate)
        => aggregate.Status == EvacuationRouteStatus.Retired;
}
