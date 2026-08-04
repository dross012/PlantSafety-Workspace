// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to list all manifests that are not yet Closed.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HazardousWasteManagement.Manifests.Queries;

/// <summary>Lists all open (not yet Closed) manifests for the tenant, optionally scoped to a single site.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
public sealed record ListOpenManifestsQuery(
    Guid TenantId,
    Guid? SiteId) : IRequest<Result<ListOpenManifestsResult>>;
