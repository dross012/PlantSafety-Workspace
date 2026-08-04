// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
//  Layer:          Domain
//  File:           EvacuationRouteService.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain service for EvacuationRoute. Coordinates logic that cannot live on a
//                  single aggregate (e.g. computing route-risk score to feed RiskManagement).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Services;

/// <summary>Domain service for EvacuationRoute. Coordinates logic that cannot live on a single aggregate.</summary>
public class EvacuationRouteService : IDomainService
{
    /// <summary>
    /// Computes a normalized 0-100 route risk score from obstruction frequency and estimated egress
    /// time, for consumption by RiskManagement (risk hook). Domain services must not reference
    /// infrastructure (no HTTP/SQL/logging).
    /// </summary>
    public decimal ComputeRouteRiskScore(Aggregates.EvacuationRoute route)
    {
        var obstructionPenalty = Math.Min(route.ObstructionCount * 10m, 60m);
        var egressPenalty = route.EstimatedEgressTimeSeconds switch
        {
            <= 120 => 0m,
            <= 300 => 15m,
            <= 600 => 30m,
            _ => 40m,
        };

        return Math.Min(obstructionPenalty + egressPenalty, 100m);
    }
}
