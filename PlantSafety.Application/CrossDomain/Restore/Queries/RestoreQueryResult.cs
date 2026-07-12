// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetRestoreQuery.
// SchemaVersion: 1

using PlantSafety.Application.CrossDomain.Restore.Models;

namespace PlantSafety.Application.CrossDomain.Restore.Queries;

/// <summary>Result of <see cref="GetRestoreQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Restore items.</param>
public sealed record RestoreQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<RestoreItemDto> Items);
