// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to list active (unresolved) AccessAnomalies for the Anomaly Feed
//   dashboard.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PhysicalAccessControl.AccessAnomalies.Queries;

/// <summary>Lists active (unresolved) anomalies for the given tenant, optionally scoped to a single site.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns anomalies for all accessible sites.</param>
public sealed record ListActiveAnomaliesQuery(
    Guid TenantId,
    Guid? SiteId) : IRequest<Result<ListActiveAnomaliesResult>>;
