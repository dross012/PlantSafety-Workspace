// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve DischargePoint read-model data.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Models;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Queries;

/// <summary>Retrieves DischargePoint read-model data.</summary>
public sealed record ListDischargePointsBySiteQuery(Guid TenantId, Guid? SiteId) : IRequest<Result<ListDischargePointsBySiteQueryResult>>;

/// <summary>Result of <see cref="ListDischargePointsBySiteQuery"/>.</summary>
public sealed record ListDischargePointsBySiteQueryResult(IReadOnlyList<DischargePointItemDto> Items, DateTime GeneratedAtUtc);