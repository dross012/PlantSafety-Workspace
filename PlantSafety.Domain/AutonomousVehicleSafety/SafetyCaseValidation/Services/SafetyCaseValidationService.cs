// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.SafetyCaseValidation
//  Layer:          Domain
//  File:           SafetyCaseValidationService.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain service for SafetyCaseValidation. Coordinates logic that cannot live on a
//                  single aggregate (e.g. computing recurring revalidation cadence due dates).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.Services;

/// <summary>Domain service for SafetyCaseValidation. Coordinates logic that cannot live on a single aggregate.</summary>
public class SafetyCaseValidationService : IDomainService
{
    // Inject domain repositories and policies via constructor if needed.
    // Domain services must not reference infrastructure (no HTTP/SQL/logging).
}
