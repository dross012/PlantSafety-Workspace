// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Application
//  File:           ListPackagingSpecsResult.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Result payload for ListPackagingSpecsQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.PermitsQueryResult.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.PHMSA.Packaging.Models;

namespace PlantSafety.Application.Compliance.PHMSA.Packaging.Queries;

/// <summary>Result of <see cref="ListPackagingSpecsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching PackagingSpecification items.</param>
public sealed record ListPackagingSpecsResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PackagingSpecificationDto> Items);
