// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetEfficiencyQuery.
// SchemaVersion: 1

using PlantSafety.Application.Auditing.AuditEngagement.Efficiency.Models;

namespace PlantSafety.Application.Auditing.AuditEngagement.Efficiency.Queries;

/// <summary>Result of <see cref="GetEfficiencyQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Efficiency items.</param>
public sealed record EfficiencyQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<EfficiencyItemDto> Items);