// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Result payload for GetReportingQuery.
// SchemaVersion: 1

using PlantSafety.Application.Integrations.Reporting.Models;

namespace PlantSafety.Application.Integrations.Reporting.Queries;

/// <summary>Result of <see cref="GetReportingQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Integrations.Reporting items.</param>
public sealed record ReportingQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ReportingItemDto> Items);
