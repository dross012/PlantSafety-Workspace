// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetFacilityAssessmentQuery.
// SchemaVersion: 1

using PlantSafety.Application.Auditing.AuditEngagement.FacilityAssessment.Models;

namespace PlantSafety.Application.Auditing.AuditEngagement.FacilityAssessment.Queries;

/// <summary>Result of <see cref="GetFacilityAssessmentQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching FacilityAssessment items.</param>
public sealed record FacilityAssessmentQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<FacilityAssessmentItemDto> Items);