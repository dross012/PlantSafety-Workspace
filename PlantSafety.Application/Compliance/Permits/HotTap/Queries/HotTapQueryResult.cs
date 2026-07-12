// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetHotTapQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Permits.HotTap.Models;

namespace PlantSafety.Application.Compliance.Permits.HotTap.Queries;

/// <summary>Result of <see cref="GetHotTapQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching HotTap items.</param>
public sealed record HotTapQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<HotTapItemDto> Items);
