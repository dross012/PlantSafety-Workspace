// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetPermitToWorkQuery.
// SchemaVersion: 1

using PlantSafety.Application.ContractorSafety.PermitToWork.Models;

namespace PlantSafety.Application.ContractorSafety.PermitToWork.Queries;

/// <summary>Result of <see cref="GetPermitToWorkQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching PermitToWork items.</param>
public sealed record PermitToWorkQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PermitToWorkItemDto> Items);