// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for GetTransportVehicleByIdQuery.
// SchemaVersion: 1

using PlantSafety.Application.LogisticsTransportSafety.TransportVehicles.Models;

namespace PlantSafety.Application.LogisticsTransportSafety.TransportVehicles.Queries;

/// <summary>Result of <see cref="GetTransportVehicleByIdQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Item">The matching TransportVehicle item, or null if not found.</param>
public sealed record TransportVehicleQueryResult(
    Guid TenantId,
    DateTime GeneratedAt,
    TransportVehicleItemDto? Item);
