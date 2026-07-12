// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Result payload for GetAuditingQuery.
// SchemaVersion: 1

using PlantSafety.Application.Integrations.Auditing.Models;

namespace PlantSafety.Application.Integrations.Auditing.Queries;

/// <summary>Result of <see cref="GetAuditingQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Integrations.Auditing items.</param>
public sealed record AuditingQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<AuditingItemDto> Items);
