// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for GetCarriersQuery.
// SchemaVersion: 1

using PlantSafety.Application.LogisticsTransportSafety.Carriers.Models;

namespace PlantSafety.Application.LogisticsTransportSafety.Carriers.Queries;

/// <summary>Result of <see cref="GetCarriersQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Carrier items.</param>
public sealed record CarrierQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<CarrierItemDto> Items);
