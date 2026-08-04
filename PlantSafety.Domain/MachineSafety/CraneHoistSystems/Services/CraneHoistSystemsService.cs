// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.CraneHoistSystems
//  Layer:          Domain
//  File:           CraneHoistSystemsService.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain service for CraneHoistSystems. Coordinates logic that cannot live on a single aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.CraneHoistSystems.Services;

/// <summary>Domain service for CraneHoistSystems. Coordinates logic that cannot live on a single aggregate.</summary>
public class CraneHoistSystemsService : IDomainService
{
    // Inject domain repositories and policies via constructor if needed.
    // Domain services must not reference infrastructure (no HTTP/SQL/logging).
}
