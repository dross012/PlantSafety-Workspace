// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve DischargePermit read-model data.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Models;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Queries;

/// <summary>Retrieves DischargePermit read-model data.</summary>
public sealed record GetDischargePermitByIdQuery(Guid TenantId, Guid DischargePermitId) : IRequest<Result<GetDischargePermitByIdQueryResult>>;

/// <summary>Result of <see cref="GetDischargePermitByIdQuery"/>.</summary>
public sealed record GetDischargePermitByIdQueryResult(DischargePermitItemDto? Items, DateTime GeneratedAtUtc);