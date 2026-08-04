// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve current qualification status for a single driver.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.LogisticsTransportSafety.DriverQualifications.Queries;

/// <summary>Retrieves current DriverQualification status/history for the given driver.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="DriverUserId">Identity of the driver whose qualification status is requested.</param>
public sealed record GetDriverQualificationStatusQuery(
    Guid TenantId,
    Guid DriverUserId) : IRequest<Result<DriverQualificationQueryResult>>;
