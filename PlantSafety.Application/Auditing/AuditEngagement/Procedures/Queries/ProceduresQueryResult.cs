// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetProceduresQuery.
// SchemaVersion: 1

using PlantSafety.Application.Auditing.AuditEngagement.Procedures.Models;

namespace PlantSafety.Application.Auditing.AuditEngagement.Procedures.Queries;

/// <summary>Result of <see cref="GetProceduresQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Procedures items.</param>
public sealed record ProceduresQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ProceduresItemDto> Items);