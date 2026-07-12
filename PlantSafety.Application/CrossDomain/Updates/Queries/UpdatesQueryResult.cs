// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetUpdatesQuery.
// SchemaVersion: 1

using PlantSafety.Application.CrossDomain.Updates.Models;

namespace PlantSafety.Application.CrossDomain.Updates.Queries;

/// <summary>Result of <see cref="GetUpdatesQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Updates items.</param>
public sealed record UpdatesQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<UpdatesItemDto> Items);
