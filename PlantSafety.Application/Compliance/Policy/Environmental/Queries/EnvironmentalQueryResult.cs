// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetEnvironmentalQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Policy.Environmental.Models;

namespace PlantSafety.Application.Compliance.Policy.Environmental.Queries;

/// <summary>Result of <see cref="GetEnvironmentalQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Environmental items.</param>
public sealed record EnvironmentalQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<EnvironmentalItemDto> Items);
