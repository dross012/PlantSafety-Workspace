// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
//  Layer:          Domain
//  File:           AutonomousVehicleAssetService.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain service for AutonomousVehicleAsset. Coordinates logic that cannot live on a
//                  single aggregate (e.g. repeated-incident suspension review escalation, evaluated
//                  in conjunction with AutonomousVehicleIncident). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Services;

/// <summary>Domain service for AutonomousVehicleAsset. Coordinates logic that cannot live on a single aggregate.</summary>
public class AutonomousVehicleAssetService : IDomainService
{
    // Inject domain repositories and policies via constructor if needed.
    // Domain services must not reference infrastructure (no HTTP/SQL/logging).
}
