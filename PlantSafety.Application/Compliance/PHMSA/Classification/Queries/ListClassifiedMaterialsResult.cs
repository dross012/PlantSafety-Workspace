// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Application
//  File:           ListClassifiedMaterialsResult.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Result payload for ListClassifiedMaterialsQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.PermitsQueryResult.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.PHMSA.Classification.Models;

namespace PlantSafety.Application.Compliance.PHMSA.Classification.Queries;

/// <summary>Result of <see cref="ListClassifiedMaterialsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching HazmatMaterial items.</param>
public sealed record ListClassifiedMaterialsResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<HazmatMaterialDto> Items);
