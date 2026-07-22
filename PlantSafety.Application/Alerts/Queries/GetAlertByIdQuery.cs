// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Query to retrieve a single Alert by identifier.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Alerts.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Alerts.Queries;

/// <summary>Retrieves a single Alert by identifier.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="AlertId">Identifier of the alert to retrieve.</param>
public sealed record GetAlertByIdQuery(Guid TenantId, Guid AlertId) : IRequest<Result<AlertDto>>;
