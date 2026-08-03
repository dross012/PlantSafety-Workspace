// AUTHOR: PlantSafety Platform Team (Phase 7 §3.4 License Enforcement)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Application-layer abstraction over an in-memory, periodically-refreshed cache
//   of each tenant's SubscriptionPlan API rate limit (requests/minute), consumed synchronously by
//   the ASP.NET Core rate limiter middleware's partition-key factory (which cannot itself perform
//   async I/O). Populated by TenantRateLimitCacheRefreshService (Infrastructure, a background
//   hosted service) rather than looked up per-request, so the hot request path never blocks on a
//   database round trip to determine a tenant's rate limit.
// SchemaVersion: 1

namespace PlantSafety.Application.Abstractions.RateLimiting;

/// <summary>In-memory cache of each tenant's current API rate limit (requests/minute), for synchronous consumption by rate-limiting middleware.</summary>
public interface ITenantRateLimitCache
{
    /// <summary>Attempts to retrieve the cached rate limit (requests/minute) for the given tenant.</summary>
    bool TryGetRateLimitPerMinute(Guid tenantId, out int rateLimitPerMinute);

    /// <summary>Sets/replaces the cached rate limit (requests/minute) for the given tenant. Used by the periodic refresh service.</summary>
    void SetRateLimitPerMinute(Guid tenantId, int rateLimitPerMinute);
}
