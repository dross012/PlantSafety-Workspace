// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve DischargeLimit read-model data.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Models;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Queries;

/// <summary>Retrieves DischargeLimit read-model data.</summary>
public sealed record ListDischargeLimitsByPointQuery(Guid TenantId, Guid DischargePointId) : IRequest<Result<ListDischargeLimitsByPointQueryResult>>;

/// <summary>Result of <see cref="ListDischargeLimitsByPointQuery"/>.</summary>
public sealed record ListDischargeLimitsByPointQueryResult(IReadOnlyList<DischargeLimitItemDto> Items, DateTime GeneratedAtUtc);