// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve a single AutonomousVehicleAsset record by identifier.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleAsset.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleAsset.Queries;

/// <summary>Retrieves a single AutonomousVehicleAsset record by its identifier for the given tenant.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="AutonomousVehicleAssetId">Identifier of the asset to retrieve.</param>
public sealed record GetAutonomousVehicleAssetByIdQuery(
    Guid TenantId,
    Guid AutonomousVehicleAssetId) : IRequest<Result<AutonomousVehicleAssetItemDto?>>;
