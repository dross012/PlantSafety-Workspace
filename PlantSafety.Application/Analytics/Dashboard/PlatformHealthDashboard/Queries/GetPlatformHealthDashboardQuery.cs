// AUTHOR: PlantSafety Platform Team (Claude Code performance-optimization-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Query to retrieve PlatformHealth BI dashboard KPI data (request throughput,
//                    latency percentiles, error rate, event-bus/DLQ health) for a tenant within
//                    a date range. Handbook Phase 6 §5.3 Metrics & Dashboards.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Behaviors.Caching;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Analytics.Dashboard.PlatformHealthDashboard.Queries;

/// <summary>
/// Retrieves PlatformHealth BI dashboard KPI data for the given tenant within the inclusive UTC
/// date range [<paramref name="From"/>, <paramref name="To"/>]. Implements
/// <see cref="ICacheableQuery"/> (Handbook §3.1 Canonical Caching Rule), matching the existing
/// six unified-analytics-model dashboards' cache-key/TTL shape.
/// </summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC).</param>
/// <param name="DeadLetterLookbackDays">Lookback window, in days, for the event-bus/DLQ health summary.</param>
public sealed record GetPlatformHealthDashboardQuery(
    Guid TenantId,
    DateTime From,
    DateTime To,
    int DeadLetterLookbackDays = 7) : IRequest<Result<PlatformHealthDashboardQueryResult>>, ICacheableQuery
{
    /// <inheritdoc/>
    public string CacheKey => $"dashboard:platformhealth:{TenantId}:{From:yyyyMMddHH}:{To:yyyyMMddHH}";

    /// <inheritdoc/>
    public TimeSpan CacheExpiry => TimeSpan.FromMinutes(5);
}
