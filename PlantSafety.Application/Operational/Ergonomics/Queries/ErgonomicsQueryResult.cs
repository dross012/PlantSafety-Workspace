// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetErgonomicsQuery.
// SchemaVersion: 1

using PlantSafety.Application.Operational.Ergonomics.Models;

namespace PlantSafety.Application.Operational.Ergonomics.Queries;

/// <summary>Result of <see cref="GetErgonomicsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Ergonomics items.</param>
public sealed record ErgonomicsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ErgonomicsItemDto> Items);
