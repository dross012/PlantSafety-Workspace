// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetOperationsQuery.
// SchemaVersion: 1

using PlantSafety.Application.Auditing.AuditEngagement.Operations.Models;

namespace PlantSafety.Application.Auditing.AuditEngagement.Operations.Queries;

/// <summary>Result of <see cref="GetOperationsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Operations items.</param>
public sealed record OperationsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<OperationsItemDto> Items);