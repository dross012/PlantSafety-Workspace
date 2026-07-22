// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Query to retrieve all Alerts that have not yet been closed.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Alerts.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Alerts.Queries;

/// <summary>Retrieves all Alerts not yet closed, optionally scoped to a single site.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
public sealed record GetActiveAlertsQuery(Guid TenantId, Guid? SiteId) : IRequest<Result<GetActiveAlertsResult>>;

/// <summary>Result of <see cref="GetActiveAlertsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAtUtc">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching active alerts, most severe first.</param>
public sealed record GetActiveAlertsResult(Guid TenantId, Guid? SiteId, DateTime GeneratedAtUtc, IReadOnlyList<AlertDto> Items);
