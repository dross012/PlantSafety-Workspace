// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve the AutonomousVehicleIncident history for a given autonomous vehicle asset.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleIncident.Queries;

/// <summary>Retrieves the incident history for a given autonomous vehicle asset.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="AutonomousVehicleAssetId">Identifier of the asset to retrieve incident history for.</param>
public sealed record GetIncidentHistoryByAssetQuery(
    Guid TenantId,
    Guid AutonomousVehicleAssetId) : IRequest<Result<AutonomousVehicleIncidentQueryResult>>;
