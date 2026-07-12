// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetElectricalQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Permits.Electrical.Models;

namespace PlantSafety.Application.Compliance.Permits.Electrical.Queries;

/// <summary>Result of <see cref="GetElectricalQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Electrical items.</param>
public sealed record ElectricalQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ElectricalItemDto> Items);
