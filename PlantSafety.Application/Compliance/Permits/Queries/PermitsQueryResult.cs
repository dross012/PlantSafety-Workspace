// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Application
//  File:           PermitsQueryResult.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Result payload for GetPermitsQuery. Mirrors
//                  PlantSafety.Application.PermitToWork.PermitWorkflow.Queries.PermitWorkflowQueryResult.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Permits.Models;

namespace PlantSafety.Application.Compliance.Permits.Queries;

/// <summary>Result of <see cref="GetPermitsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Permits items.</param>
public sealed record PermitsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PermitsItemDto> Items);
