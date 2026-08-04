// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve IncidentNotification dispatch status for a tenant/site,
// optionally scoped to a single incident, within a date range.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PublicSafetyCommunication.IncidentNotificationDispatch.Queries;

/// <summary>Retrieves IncidentNotification dispatch status data for the given tenant/site/incident within a date range.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="IncidentId">Optional incident filter. Null returns data for all incidents.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC).</param>
public sealed record GetIncidentNotificationStatusQuery(
    Guid TenantId,
    Guid? SiteId,
    Guid? IncidentId,
    DateTime From,
    DateTime To) : IRequest<Result<GetIncidentNotificationStatusQueryResult>>;
