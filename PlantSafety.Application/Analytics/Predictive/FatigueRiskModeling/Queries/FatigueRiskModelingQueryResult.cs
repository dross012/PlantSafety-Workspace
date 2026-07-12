// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetFatigueRiskModelingQuery.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Predictive.FatigueRiskModeling.Models;

namespace PlantSafety.Application.Analytics.Predictive.FatigueRiskModeling.Queries;

/// <summary>Result of <see cref="GetFatigueRiskModelingQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching FatigueRiskModeling items.</param>
public sealed record FatigueRiskModelingQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<FatigueRiskModelingItemDto> Items);
