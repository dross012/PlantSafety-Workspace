// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Domain
//  File:           EmergencyDrillPlanningService.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain service for EmergencyDrillPlanning. Coordinates logic that cannot live on a single aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Services;

/// <summary>Domain service for EmergencyDrillPlanning. Coordinates logic that cannot live on a single aggregate.</summary>
public class EmergencyDrillPlanningService : IDomainService
{
    // Inject domain repositories and policies via constructor if needed.
    // Domain services must not reference infrastructure (no HTTP/SQL/logging).
}