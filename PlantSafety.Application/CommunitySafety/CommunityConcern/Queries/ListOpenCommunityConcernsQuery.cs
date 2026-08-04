// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to list open (not Resolved/Archived) CommunityConcern records for a tenant/site.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CommunitySafety.CommunityConcern.Queries;

/// <summary>Lists open CommunityConcern records for the given tenant, optionally scoped to a single site.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
public sealed record ListOpenCommunityConcernsQuery(
    Guid TenantId,
    Guid? SiteId) : IRequest<Result<CommunityConcernsQueryResult>>;
