// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-20
// FILE DESCRIPTION: Result payload for GetWellnessQuery.
// SchemaVersion: 1

using PlantSafety.Application.Operational.Wellness.Models;

namespace PlantSafety.Application.Operational.Wellness.Queries;

/// <summary>Result of <see cref="GetWellnessQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Wellness items.</param>
public sealed record WellnessQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<WellnessItemDto> Items);