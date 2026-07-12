// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetBehaviorQuery.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Culture.Behavior.Models;

namespace PlantSafety.Application.Analytics.Culture.Behavior.Queries;

/// <summary>Result of <see cref="GetBehaviorQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Behavior items.</param>
public sealed record BehaviorQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<BehaviorItemDto> Items);
