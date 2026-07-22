// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetPPEInventoryQuery.
// SchemaVersion: 1

using PlantSafety.Application.PPE.PPEInventory.Models;

namespace PlantSafety.Application.PPE.PPEInventory.Queries;

/// <summary>Result of <see cref="GetPPEInventoryQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching PPEInventory items.</param>
public sealed record PPEInventoryQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PPEInventoryItemDto> Items);