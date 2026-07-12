// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetRootCauseAnalysisQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Models;

namespace PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Queries;

/// <summary>Result of <see cref="GetRootCauseAnalysisQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching RootCauseAnalysis items.</param>
public sealed record RootCauseAnalysisQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<RootCauseAnalysisItemDto> Items);
