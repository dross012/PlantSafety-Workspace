// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve the current safety-case validation status for an autonomous vehicle asset.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.AutonomousVehicleSafety.SafetyCaseValidation.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AutonomousVehicleSafety.SafetyCaseValidation.Queries;

/// <summary>Retrieves the current safety-case validation status for a given autonomous vehicle asset.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="AutonomousVehicleAssetId">Identifier of the asset to retrieve safety-case status for.</param>
public sealed record GetSafetyCaseStatusQuery(
    Guid TenantId,
    Guid AutonomousVehicleAssetId) : IRequest<Result<SafetyCaseValidationItemDto?>>;
