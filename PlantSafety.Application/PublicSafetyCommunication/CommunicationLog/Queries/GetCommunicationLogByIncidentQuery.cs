// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve the full communication log for a single incident, ordered
// chronologically, for after-action review.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PublicSafetyCommunication.CommunicationLog.Queries;

/// <summary>Retrieves the communication log for the given tenant/incident.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="IncidentId">Incident identifier to retrieve the communication log for.</param>
public sealed record GetCommunicationLogByIncidentQuery(
    Guid TenantId,
    Guid IncidentId) : IRequest<Result<GetCommunicationLogByIncidentQueryResult>>;
