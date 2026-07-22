// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Query to retrieve Corrective Actions (CAPA) read-model data for a tenant/site within a date range.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AccidentManagement.CorrectiveActionsCAPA.Queries;

/// <summary>
/// Retrieves Corrective Actions (CAPA) data for the given tenant, optionally scoped to a single site,
/// within the inclusive UTC date range [<paramref name="From"/>, <paramref name="To"/>].
/// </summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC).</param>
public sealed record GetCorrectiveActionsCAPAQuery(
    Guid TenantId,
    Guid? SiteId,
    DateTime From,
    DateTime To) : IRequest<Result<CorrectiveActionsCAPAQueryResult>>;