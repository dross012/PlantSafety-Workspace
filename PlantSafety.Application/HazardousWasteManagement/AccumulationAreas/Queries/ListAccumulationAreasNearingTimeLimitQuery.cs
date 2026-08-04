// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to list AccumulationArea records nearing their regulatory time limit.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HazardousWasteManagement.AccumulationAreas.Queries;

/// <summary>Lists accumulation areas with containers approaching their RCRA-style accumulation time limit. Feeds the Accumulation Area Time-Limit Board dashboard.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
public sealed record ListAccumulationAreasNearingTimeLimitQuery(
    Guid TenantId,
    Guid? SiteId) : IRequest<Result<ListAccumulationAreasNearingTimeLimitResult>>;
