// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EgressSimulations
//  Layer:          Domain
//  File:           EgressSimulationBottleneckAnalysisService.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain service implementing the Bottleneck Analysis subdomain's detection
//                  logic: given a completed egress-simulation run's estimated-vs-simulated time
//                  delta, determines whether a bottleneck should be raised against the modeled
//                  route. Coordinates across the EgressSimulationRun and EvacuationRoute
//                  aggregates -- logic that cannot live on a single aggregate. Domain services
//                  must not reference infrastructure (no HTTP/SQL/logging). Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Aggregates;

namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Services;

/// <summary>Domain service that detects evacuation bottlenecks from completed egress-simulation results.</summary>
public class EgressSimulationBottleneckAnalysisService : IDomainService
{
    /// <summary>Delay, in seconds, above a route's estimated egress time that is treated as a bottleneck.</summary>
    public const int BottleneckDelayThresholdSeconds = 60;

    /// <summary>
    /// Returns <see langword="true"/> and the delay in seconds when a completed simulation run's
    /// simulated time exceeds the route's estimated egress time by more than
    /// <see cref="BottleneckDelayThresholdSeconds"/>.
    /// </summary>
    public bool TryDetectBottleneck(EgressSimulationRun run, int routeEstimatedEgressTimeSeconds, out int delaySeconds)
    {
        delaySeconds = 0;

        if (run.Status != PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Enums.EgressSimulationStatus.Completed
            || run.SimulatedEvacuationTimeSeconds is null)
            return false;

        var delta = run.SimulatedEvacuationTimeSeconds.Value - routeEstimatedEgressTimeSeconds;
        if (delta <= BottleneckDelayThresholdSeconds)
            return false;

        delaySeconds = delta;
        return true;
    }
}
