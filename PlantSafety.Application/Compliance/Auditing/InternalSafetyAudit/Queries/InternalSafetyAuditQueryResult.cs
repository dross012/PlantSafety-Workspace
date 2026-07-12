// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetInternalSafetyAuditQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Auditing.InternalSafetyAudit.Models;

namespace PlantSafety.Application.Compliance.Auditing.InternalSafetyAudit.Queries;

/// <summary>Result of <see cref="GetInternalSafetyAuditQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching InternalSafetyAudit items.</param>
public sealed record InternalSafetyAuditQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<InternalSafetyAuditItemDto> Items);
