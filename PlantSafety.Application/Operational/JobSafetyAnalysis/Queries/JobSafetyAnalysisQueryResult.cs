// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetJobSafetyAnalysisQuery.
// SchemaVersion: 1

using PlantSafety.Application.Operational.JobSafetyAnalysis.Models;

namespace PlantSafety.Application.Operational.JobSafetyAnalysis.Queries;

/// <summary>Result of <see cref="GetJobSafetyAnalysisQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching JobSafetyAnalysis items.</param>
public sealed record JobSafetyAnalysisQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<JobSafetyAnalysisItemDto> Items);
