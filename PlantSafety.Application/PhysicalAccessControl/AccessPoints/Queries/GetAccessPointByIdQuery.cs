// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve a single AccessPoint by identity.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PhysicalAccessControl.AccessPoints.Queries;

/// <summary>Retrieves a single AccessPoint by identity, scoped to the tenant.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="AccessPointId">Identity of the AccessPoint to retrieve.</param>
public sealed record GetAccessPointByIdQuery(
    Guid TenantId,
    Guid AccessPointId) : IRequest<Result<GetAccessPointByIdResult>>;
