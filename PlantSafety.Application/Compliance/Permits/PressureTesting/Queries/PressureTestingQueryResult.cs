// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetPressureTestingQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Permits.PressureTesting.Models;

namespace PlantSafety.Application.Compliance.Permits.PressureTesting.Queries;

/// <summary>Result of <see cref="GetPressureTestingQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching PressureTesting items.</param>
public sealed record PressureTestingQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PressureTestingItemDto> Items);
