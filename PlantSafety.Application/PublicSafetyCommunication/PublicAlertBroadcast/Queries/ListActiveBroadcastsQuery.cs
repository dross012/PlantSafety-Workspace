// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to list active (Sent/Delivered) PublicAlertBroadcast records for a
// tenant/site.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PublicSafetyCommunication.PublicAlertBroadcast.Queries;

/// <summary>Lists active PublicAlertBroadcast records for the given tenant, optionally scoped to a single site.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
public sealed record ListActiveBroadcastsQuery(
    Guid TenantId,
    Guid? SiteId) : IRequest<Result<ListActiveBroadcastsQueryResult>>;
