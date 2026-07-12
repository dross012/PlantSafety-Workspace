// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetRadiographyQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Permits.Radiography.Models;

namespace PlantSafety.Application.Compliance.Permits.Radiography.Queries;

/// <summary>Result of <see cref="GetRadiographyQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Radiography items.</param>
public sealed record RadiographyQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<RadiographyItemDto> Items);
