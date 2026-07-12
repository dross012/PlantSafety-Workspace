// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetAnnualAuditQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.RegulatoryCalendar.Deadlines.AnnualAudit.Models;

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Deadlines.AnnualAudit.Queries;

/// <summary>Result of <see cref="GetAnnualAuditQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching AnnualAudit items.</param>
public sealed record AnnualAuditQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<AnnualAuditItemDto> Items);
