// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetPermissionsQuery.
// SchemaVersion: 1

using PlantSafety.Application.CrossDomain.Permissions.Models;

namespace PlantSafety.Application.CrossDomain.Permissions.Queries;

/// <summary>Result of <see cref="GetPermissionsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Permissions items.</param>
public sealed record PermissionsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PermissionsItemDto> Items);
