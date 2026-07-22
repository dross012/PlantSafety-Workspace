// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetPpeComplianceRateQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyCulture.CultureAssessment.PpeComplianceRate.Models;

namespace PlantSafety.Application.SafetyCulture.CultureAssessment.PpeComplianceRate.Queries;

/// <summary>Result of <see cref="GetPpeComplianceRateQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching PpeComplianceRate items.</param>
public sealed record PpeComplianceRateQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PpeComplianceRateItemDto> Items);