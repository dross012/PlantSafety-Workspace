// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Application
//  File:           GetGlobalSafetyInsightsQuery.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Query retrieving the most recently computed GlobalSafetyInsights snapshot (and
//                  optionally its recent history) for a tenant -- the entry point the Phase 8
//                  dashboard (built by a later agent) will call. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Behaviors.Caching;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.GlobalSafetyInsights.Queries;

/// <summary>
/// Retrieves the most recently computed GlobalSafetyInsights snapshot for
/// <paramref name="TenantId"/>, plus its computation history within [<paramref name="From"/>,
/// <paramref name="To"/>].
/// Implements <see cref="ICacheableQuery"/> (Handbook §3.1 Canonical Caching Rule) -- read-heavy,
/// short-TTL BI aggregation, safe to serve briefly stale via <see cref="CachingBehavior{TRequest,TResponse}"/>,
/// matching the existing GetRiskDashboardQuery/GetSafetyDashboardQuery precedent's cache-key/TTL shape.
/// </summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="From">Inclusive start of the history date range (UTC).</param>
/// <param name="To">Inclusive end of the history date range (UTC).</param>
public sealed record GetGlobalSafetyInsightsQuery(
    Guid TenantId,
    DateTime From,
    DateTime To) : IRequest<Result<GlobalSafetyInsightsQueryResult>>, ICacheableQuery
{
    /// <inheritdoc/>
    public string CacheKey => $"dashboard:globalsafetyinsights:{TenantId}:{From:yyyyMMdd}:{To:yyyyMMdd}";

    /// <inheritdoc/>
    public TimeSpan CacheExpiry => TimeSpan.FromMinutes(5);
}
