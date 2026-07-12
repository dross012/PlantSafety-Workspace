// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetLockoutTagoutQuery.
// SchemaVersion: 1

using PlantSafety.Application.Operational.LockoutTagout.Models;

namespace PlantSafety.Application.Operational.LockoutTagout.Queries;

/// <summary>Result of <see cref="GetLockoutTagoutQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching LockoutTagout items.</param>
public sealed record LockoutTagoutQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<LockoutTagoutItemDto> Items);
