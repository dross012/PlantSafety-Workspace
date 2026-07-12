// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetUsersQuery.
// SchemaVersion: 1

using PlantSafety.Application.CrossDomain.Users.Models;

namespace PlantSafety.Application.CrossDomain.Users.Queries;

/// <summary>Result of <see cref="GetUsersQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Users items.</param>
public sealed record UsersQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<UsersItemDto> Items);
