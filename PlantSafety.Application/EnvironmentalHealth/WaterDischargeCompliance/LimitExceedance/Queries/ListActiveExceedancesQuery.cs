// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve LimitExceedance read-model data.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Models;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Queries;

/// <summary>Retrieves LimitExceedance read-model data.</summary>
public sealed record ListActiveExceedancesQuery(Guid TenantId, Guid? SiteId) : IRequest<Result<ListActiveExceedancesQueryResult>>;

/// <summary>Result of <see cref="ListActiveExceedancesQuery"/>.</summary>
public sealed record ListActiveExceedancesQueryResult(IReadOnlyList<LimitExceedanceItemDto> Items, DateTime GeneratedAtUtc);