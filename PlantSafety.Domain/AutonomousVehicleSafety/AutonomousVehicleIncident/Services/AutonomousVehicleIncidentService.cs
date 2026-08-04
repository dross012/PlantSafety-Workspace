// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleIncident
//  Layer:          Domain
//  File:           AutonomousVehicleIncidentService.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain service for AutonomousVehicleIncident. Coordinates logic that cannot live
//                  on a single aggregate (e.g. repeated-incident escalation to
//                  AutonomousVehicleAsset.Suspend, and autonomous-mode-suspension-frequency KPI
//                  aggregation). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Services;

/// <summary>Domain service for AutonomousVehicleIncident. Coordinates logic that cannot live on a single aggregate.</summary>
public class AutonomousVehicleIncidentService : IDomainService
{
    // Inject domain repositories and policies via constructor if needed.
    // Domain services must not reference infrastructure (no HTTP/SQL/logging).
}
