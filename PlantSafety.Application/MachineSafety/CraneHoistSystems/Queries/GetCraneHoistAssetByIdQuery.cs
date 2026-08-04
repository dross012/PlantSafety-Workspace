// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve a single CraneHoistSystems asset record by identifier.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Models;

namespace PlantSafety.Application.MachineSafety.CraneHoistSystems.Queries;

/// <summary>Retrieves a single CraneHoistSystems asset by identifier.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="CraneHoistSystemsId">Identity of the asset to retrieve.</param>
public sealed record GetCraneHoistAssetByIdQuery(
    Guid TenantId,
    Guid CraneHoistSystemsId) : IRequest<Result<CraneHoistSystemsItemDto?>>;
