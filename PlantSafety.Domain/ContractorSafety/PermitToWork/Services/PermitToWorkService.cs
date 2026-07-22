// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PermitToWork
//  Layer:          Domain
//  File:           PermitToWorkService.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain service for PermitToWork. Coordinates logic that cannot live on a single aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PermitToWork.Services;

/// <summary>Domain service for PermitToWork. Coordinates logic that cannot live on a single aggregate.</summary>
public class PermitToWorkService : IDomainService
{
    // Inject domain repositories and policies via constructor if needed.
    // Domain services must not reference infrastructure (no HTTP/SQL/logging).
}