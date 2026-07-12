// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetComplianceAuditQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Auditing.ComplianceAudit.Models;

namespace PlantSafety.Application.Compliance.Auditing.ComplianceAudit.Queries;

/// <summary>Result of <see cref="GetComplianceAuditQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching ComplianceAudit items.</param>
public sealed record ComplianceAuditQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ComplianceAuditItemDto> Items);
