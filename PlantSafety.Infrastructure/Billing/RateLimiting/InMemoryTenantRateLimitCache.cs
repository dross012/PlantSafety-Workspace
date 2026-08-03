// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.RateLimiting
//  Layer:          Infrastructure
//  File:           InMemoryTenantRateLimitCache.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.4 License Enforcement)
//  SchemaVersion:  2026.1
//  Description:    Process-local ITenantRateLimitCache implementation, backed by a
//                  ConcurrentDictionary. Populated by TenantRateLimitCacheRefreshService.
//                  Additive-only evolution.
// ============================================================================
using System.Collections.Concurrent;
using PlantSafety.Application.Abstractions.RateLimiting;

namespace PlantSafety.Infrastructure.Billing.RateLimiting;

/// <summary>Process-local, in-memory <see cref="ITenantRateLimitCache"/>.</summary>
public sealed class InMemoryTenantRateLimitCache : ITenantRateLimitCache
{
    private readonly ConcurrentDictionary<Guid, int> _rateLimits = new();

    /// <inheritdoc/>
    public bool TryGetRateLimitPerMinute(Guid tenantId, out int rateLimitPerMinute) => _rateLimits.TryGetValue(tenantId, out rateLimitPerMinute);

    /// <inheritdoc/>
    public void SetRateLimitPerMinute(Guid tenantId, int rateLimitPerMinute) => _rateLimits[tenantId] = rateLimitPerMinute;
}
