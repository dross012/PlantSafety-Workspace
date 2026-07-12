// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetReportingQuery.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Reporting.Models;

namespace PlantSafety.Application.Analytics.Reporting.Queries;

/// <summary>Result of <see cref="GetReportingQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Reporting items.</param>
public sealed record ReportingQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ReportingItemDto> Items);
