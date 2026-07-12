// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetPermitsToWorkQuery.
// SchemaVersion: 1

using PlantSafety.Application.Operational.PermitsToWork.Models;

namespace PlantSafety.Application.Operational.PermitsToWork.Queries;

/// <summary>Result of <see cref="GetPermitsToWorkQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching PermitsToWork items.</param>
public sealed record PermitsToWorkQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PermitsToWorkItemDto> Items);
