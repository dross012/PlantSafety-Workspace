// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetRegulatoryComplianceQuery.
// SchemaVersion: 1

using PlantSafety.Application.Auditing.AuditEngagement.RegulatoryCompliance.Models;

namespace PlantSafety.Application.Auditing.AuditEngagement.RegulatoryCompliance.Queries;

/// <summary>Result of <see cref="GetRegulatoryComplianceQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching RegulatoryCompliance items.</param>
public sealed record RegulatoryComplianceQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<RegulatoryComplianceItemDto> Items);