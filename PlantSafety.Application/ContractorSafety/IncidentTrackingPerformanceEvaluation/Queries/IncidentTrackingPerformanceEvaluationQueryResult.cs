// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetIncidentTrackingPerformanceEvaluationQuery.
// SchemaVersion: 1

using PlantSafety.Application.ContractorSafety.IncidentTrackingPerformanceEvaluation.Models;

namespace PlantSafety.Application.ContractorSafety.IncidentTrackingPerformanceEvaluation.Queries;

/// <summary>Result of <see cref="GetIncidentTrackingPerformanceEvaluationQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching IncidentTrackingPerformanceEvaluation items.</param>
public sealed record IncidentTrackingPerformanceEvaluationQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<IncidentTrackingPerformanceEvaluationItemDto> Items);