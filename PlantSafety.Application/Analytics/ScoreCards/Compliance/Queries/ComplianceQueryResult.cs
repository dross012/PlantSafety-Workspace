// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetComplianceQuery.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.ScoreCards.Compliance.Models;

namespace PlantSafety.Application.Analytics.ScoreCards.Compliance.Queries;

/// <summary>Result of <see cref="GetComplianceQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Compliance items.</param>
public sealed record ComplianceQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ComplianceItemDto> Items);
