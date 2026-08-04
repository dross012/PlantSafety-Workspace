// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.DriverQualifications
//  Layer:          Domain
//  File:           DriverQualificationService.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain service for DriverQualifications. Coordinates logic that cannot live on a
//                  single aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Services;

/// <summary>Domain service for DriverQualifications. Coordinates logic that cannot live on a single aggregate.</summary>
public class DriverQualificationService : IDomainService
{
    // Inject domain repositories and policies via constructor if needed.
    // Domain services must not reference infrastructure (no HTTP/SQL/logging).
}
