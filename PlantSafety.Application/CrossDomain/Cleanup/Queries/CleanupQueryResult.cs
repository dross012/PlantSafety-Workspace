// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetCleanupQuery.
// SchemaVersion: 1

using PlantSafety.Application.CrossDomain.Cleanup.Models;

namespace PlantSafety.Application.CrossDomain.Cleanup.Queries;

/// <summary>Result of <see cref="GetCleanupQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Cleanup items.</param>
public sealed record CleanupQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<CleanupItemDto> Items);
