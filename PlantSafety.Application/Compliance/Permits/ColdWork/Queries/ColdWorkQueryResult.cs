// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetColdWorkQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Permits.ColdWork.Models;

namespace PlantSafety.Application.Compliance.Permits.ColdWork.Queries;

/// <summary>Result of <see cref="GetColdWorkQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching ColdWork items.</param>
public sealed record ColdWorkQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ColdWorkItemDto> Items);
