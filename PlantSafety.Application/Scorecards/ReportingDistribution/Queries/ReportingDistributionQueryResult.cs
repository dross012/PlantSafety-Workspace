// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetReportingDistributionQuery.
// SchemaVersion: 1

using PlantSafety.Application.Scorecards.ReportingDistribution.Models;

namespace PlantSafety.Application.Scorecards.ReportingDistribution.Queries;

/// <summary>Result of <see cref="GetReportingDistributionQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Reporting &amp; Distribution items.</param>
public sealed record ReportingDistributionQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ReportingDistributionItemDto> Items);