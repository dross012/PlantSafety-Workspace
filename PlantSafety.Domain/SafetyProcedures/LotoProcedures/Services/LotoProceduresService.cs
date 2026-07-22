// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.LotoProcedures
//  Layer:          Domain
//  File:           LotoProceduresService.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain service for LotoProcedures. Coordinates logic that cannot live on a single aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.LotoProcedures.Services;

/// <summary>Domain service for LotoProcedures. Coordinates logic that cannot live on a single aggregate.</summary>
public class LotoProceduresService : IDomainService
{
    // Inject domain repositories and policies via constructor if needed.
    // Domain services must not reference infrastructure (no HTTP/SQL/logging).
}