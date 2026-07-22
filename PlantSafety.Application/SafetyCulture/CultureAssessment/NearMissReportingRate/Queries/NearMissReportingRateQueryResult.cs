// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetNearMissReportingRateQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyCulture.CultureAssessment.NearMissReportingRate.Models;

namespace PlantSafety.Application.SafetyCulture.CultureAssessment.NearMissReportingRate.Queries;

/// <summary>Result of <see cref="GetNearMissReportingRateQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching NearMissReportingRate items.</param>
public sealed record NearMissReportingRateQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<NearMissReportingRateItemDto> Items);