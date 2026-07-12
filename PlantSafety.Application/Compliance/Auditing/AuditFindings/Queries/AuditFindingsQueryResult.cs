// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetAuditFindingsQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Auditing.AuditFindings.Models;

namespace PlantSafety.Application.Compliance.Auditing.AuditFindings.Queries;

/// <summary>Result of <see cref="GetAuditFindingsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching AuditFindings items.</param>
public sealed record AuditFindingsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<AuditFindingItemDto> Items);
