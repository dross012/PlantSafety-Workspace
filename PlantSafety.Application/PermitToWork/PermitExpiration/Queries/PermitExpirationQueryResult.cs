// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetPermitExpirationQuery.
// SchemaVersion: 1

using PlantSafety.Application.PermitToWork.PermitExpiration.Models;

namespace PlantSafety.Application.PermitToWork.PermitExpiration.Queries;

/// <summary>Result of <see cref="GetPermitExpirationQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching PermitExpiration items.</param>
public sealed record PermitExpirationQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PermitExpirationItemDto> Items);