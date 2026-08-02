// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Application
//  File:           EngagementRecordsQueryResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for GetEngagementRecordsQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.PermitsQueryResult.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Engagement.Models;

namespace PlantSafety.Application.Analytics.Engagement.Queries;

/// <summary>Result of <see cref="GetEngagementRecordsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Engagement items.</param>
public sealed record EngagementRecordsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<EngagementItemDto> Items);
