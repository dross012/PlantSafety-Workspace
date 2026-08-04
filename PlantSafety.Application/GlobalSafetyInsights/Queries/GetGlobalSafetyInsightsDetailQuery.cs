// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Application
//  File:           GetGlobalSafetyInsightsDetailQuery.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §5 Global Safety Dashboard)
//  SchemaVersion:  2026.1
//  Description:    Query retrieving the full detail (per-scope risk scores, anomaly detail,
//                  recommendation detail) of the most recently computed GlobalSafetyInsights
//                  snapshot for a tenant -- backs the Global Safety Dashboard's anomaly-map and
//                  region-heatmap widgets. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Behaviors.Caching;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.GlobalSafetyInsights.Models;

namespace PlantSafety.Application.GlobalSafetyInsights.Queries;

/// <summary>
/// Retrieves the full detail of the most recently computed GlobalSafetyInsights snapshot for
/// <paramref name="TenantId"/>.
/// Implements <see cref="ICacheableQuery"/> (Handbook §3.1 Canonical Caching Rule) -- read-heavy,
/// short-TTL BI aggregation, safe to serve briefly stale via <see cref="CachingBehavior{TRequest,TResponse}"/>,
/// matching the existing GetRiskDashboardQuery/GetSafetyDashboardQuery precedent's cache-key/TTL shape.
/// </summary>
/// <param name="TenantId">Tenant identifier resolved server-side from the operation context.</param>
public sealed record GetGlobalSafetyInsightsDetailQuery(Guid TenantId)
    : IRequest<Result<GlobalSafetyInsightsDetailDto?>>, ICacheableQuery
{
    /// <inheritdoc/>
    public string CacheKey => $"dashboard:globalsafetyinsights:detail:{TenantId}";

    /// <inheritdoc/>
    public TimeSpan CacheExpiry => TimeSpan.FromMinutes(5);
}
