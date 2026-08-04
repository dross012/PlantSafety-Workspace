// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve a single TransportVehicle read-model record by identity.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.LogisticsTransportSafety.TransportVehicles.Queries;

/// <summary>Retrieves a single TransportVehicle record for the given tenant and identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="TransportVehicleId">Identity of the TransportVehicle record to retrieve.</param>
public sealed record GetTransportVehicleByIdQuery(
    Guid TenantId,
    Guid TransportVehicleId) : IRequest<Result<TransportVehicleQueryResult>>;
