// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.TsdfFacilities
//  Layer:          Domain
//  File:           TsdfFacilityService.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain service for TsdfFacility. Coordinates logic that cannot live on a single aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Services;

/// <summary>Domain service for TsdfFacility. Coordinates logic that cannot live on a single aggregate.</summary>
public class TsdfFacilityService : IDomainService
{
    // Inject domain repositories and policies via constructor if needed.
    // Domain services must not reference infrastructure (no HTTP/SQL/logging).
}
