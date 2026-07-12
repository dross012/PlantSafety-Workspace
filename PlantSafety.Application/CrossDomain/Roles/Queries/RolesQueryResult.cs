// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetRolesQuery.
// SchemaVersion: 1

using PlantSafety.Application.CrossDomain.Roles.Models;

namespace PlantSafety.Application.CrossDomain.Roles.Queries;

/// <summary>Result of <see cref="GetRolesQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Roles items.</param>
public sealed record RolesQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<RolesItemDto> Items);
