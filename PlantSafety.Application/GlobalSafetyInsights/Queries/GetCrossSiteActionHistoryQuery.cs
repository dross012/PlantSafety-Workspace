// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Application
//  File:           GetCrossSiteActionHistoryQuery.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §5 Global Safety Dashboard)
//  SchemaVersion:  2026.1
//  Description:    Query listing cross-site coordinated-action audit ledger entries for a tenant --
//                  backs the Global Safety Dashboard's corrective-action tracker and incident-
//                  timeline widgets. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Behaviors.Caching;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.GlobalSafetyInsights.Models;

namespace PlantSafety.Application.GlobalSafetyInsights.Queries;

/// <summary>
/// Lists cross-site coordinated-action audit entries for <paramref name="TenantId"/> within
/// [<paramref name="From"/>, <paramref name="To"/>].
/// Implements <see cref="ICacheableQuery"/> (Handbook §3.1 Canonical Caching Rule) -- read-heavy,
/// short-TTL BI aggregation, safe to serve briefly stale via <see cref="CachingBehavior{TRequest,TResponse}"/>,
/// matching the existing GetRiskDashboardQuery/GetSafetyDashboardQuery precedent's cache-key/TTL shape.
/// </summary>
/// <param name="TenantId">Tenant identifier resolved server-side from the operation context.</param>
/// <param name="From">Inclusive UTC start of the date range.</param>
/// <param name="To">Inclusive UTC end of the date range.</param>
public sealed record GetCrossSiteActionHistoryQuery(
    Guid TenantId,
    DateTime From,
    DateTime To) : IRequest<Result<IReadOnlyList<CrossSiteActionLogEntryDto>>>, ICacheableQuery
{
    /// <inheritdoc/>
    public string CacheKey => $"dashboard:globalsafetyinsights:crosssiteactions:{TenantId}:{From:yyyyMMdd}:{To:yyyyMMdd}";

    /// <inheritdoc/>
    public TimeSpan CacheExpiry => TimeSpan.FromMinutes(5);
}
