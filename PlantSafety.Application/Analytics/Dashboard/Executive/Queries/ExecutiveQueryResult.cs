// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetExecutiveQuery.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Dashboard.Executive.Models;

namespace PlantSafety.Application.Analytics.Dashboard.Executive.Queries;

/// <summary>Result of <see cref="GetExecutiveQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Executive items.</param>
public sealed record ExecutiveQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ExecutiveItemDto> Items);
