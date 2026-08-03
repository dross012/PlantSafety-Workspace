// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Query to retrieve ContinuousImprovement BI dashboard KPI data for a tenant/site within a date range.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Behaviors.Caching;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Analytics.Dashboard.ContinuousImprovementDashboard.Queries;

/// <summary>
/// Retrieves ContinuousImprovement BI dashboard KPI data for the given tenant, optionally scoped
/// to a single site, within the inclusive UTC date range [<paramref name="From"/>, <paramref name="To"/>].
/// Implements <see cref="ICacheableQuery"/> (Handbook §3.1 Canonical Caching Rule) -- read-heavy,
/// short-TTL BI aggregation, safe to serve briefly stale via <see cref="CachingBehavior{TRequest,TResponse}"/>,
/// matching the existing GetSafetyDashboardQuery precedent's cache-key/TTL shape.
/// </summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC).</param>
public sealed record GetContinuousImprovementDashboardQuery(
    Guid TenantId,
    Guid? SiteId,
    DateTime From,
    DateTime To) : IRequest<Result<ContinuousImprovementDashboardQueryResult>>, ICacheableQuery
{
    /// <inheritdoc/>
    public string CacheKey => $"dashboard:continuousimprovement:{TenantId}:{SiteId?.ToString() ?? "all"}:{From:yyyyMMdd}:{To:yyyyMMdd}";

    /// <inheritdoc/>
    public TimeSpan CacheExpiry => TimeSpan.FromMinutes(5);
}
