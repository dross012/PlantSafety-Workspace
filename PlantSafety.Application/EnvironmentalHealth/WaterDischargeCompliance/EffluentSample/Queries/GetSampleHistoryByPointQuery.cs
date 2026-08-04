// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve EffluentSample read-model data.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Models;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Queries;

/// <summary>Retrieves EffluentSample read-model data.</summary>
public sealed record GetSampleHistoryByPointQuery(Guid TenantId, Guid DischargePointId, DateTime From, DateTime To) : IRequest<Result<GetSampleHistoryByPointQueryResult>>;

/// <summary>Result of <see cref="GetSampleHistoryByPointQuery"/>.</summary>
public sealed record GetSampleHistoryByPointQueryResult(IReadOnlyList<EffluentSampleItemDto> Items, DateTime GeneratedAtUtc);