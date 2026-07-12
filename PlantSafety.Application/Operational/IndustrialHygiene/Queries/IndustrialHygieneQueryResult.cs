// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetIndustrialHygieneQuery.
// SchemaVersion: 1

using PlantSafety.Application.Operational.IndustrialHygiene.Models;

namespace PlantSafety.Application.Operational.IndustrialHygiene.Queries;

/// <summary>Result of <see cref="GetIndustrialHygieneQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching IndustrialHygiene items.</param>
public sealed record IndustrialHygieneQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<IndustrialHygieneItemDto> Items);
