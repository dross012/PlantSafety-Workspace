// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetExcavationQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Permits.Excavation.Models;

namespace PlantSafety.Application.Compliance.Permits.Excavation.Queries;

/// <summary>Result of <see cref="GetExcavationQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Excavation items.</param>
public sealed record ExcavationQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ExcavationItemDto> Items);
