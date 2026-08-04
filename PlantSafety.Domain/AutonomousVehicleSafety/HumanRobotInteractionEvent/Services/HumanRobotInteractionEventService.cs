// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.HumanRobotInteractionEvent
//  Layer:          Domain
//  File:           HumanRobotInteractionEventService.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain service for HumanRobotInteractionEvent. Coordinates logic that cannot live
//                  on a single aggregate (e.g. mean-distance-between-near-misses KPI aggregation).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.Services;

/// <summary>Domain service for HumanRobotInteractionEvent. Coordinates logic that cannot live on a single aggregate.</summary>
public class HumanRobotInteractionEventService : IDomainService
{
    // Inject domain repositories and policies via constructor if needed.
    // Domain services must not reference infrastructure (no HTTP/SQL/logging).
}
