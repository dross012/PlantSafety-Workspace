// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetRiskAssessmentsQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.ManagementOfChange.RiskAssessments.Models;

namespace PlantSafety.Application.Compliance.ManagementOfChange.RiskAssessments.Queries;

/// <summary>Result of <see cref="GetRiskAssessmentsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching RiskAssessments items.</param>
public sealed record RiskAssessmentsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<RiskAssessmentsItemDto> Items);
