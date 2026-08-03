// ============================================================================
// File: RateLimitingExtensions.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Phase 7 §3.4 License Enforcement)
// Created: 2026-08-03
// Description: Real, per-tenant ASP.NET Core rate limiting (System.Threading.RateLimiting),
//              keyed by the authenticated caller's tenant and that tenant's SubscriptionPlan
//              ApiRateLimitPerMinute (Phase 7 §3.2 Subscription Plans). Previously NO
//              AddRateLimiter()/UseRateLimiter() call existed anywhere in this solution
//              (verified by solution-wide search) -- API traffic was completely unthrottled at
//              the application layer regardless of tenant or plan.
//
//              The partition-key factory below is registered as a synchronous delegate (a hard
//              constraint of RateLimitPartition.Get<T>) and therefore reads the tenant's current
//              rate limit from ITenantRateLimitCache -- an in-memory cache kept fresh by
//              TenantRateLimitCacheRefreshService (Infrastructure), never performing I/O on the
//              hot request path. Unauthenticated requests and requests with no resolvable
//              "tenant_id" claim fall back to a conservative global partition sized to the Trial
//              plan's limit, rather than being exempted from rate limiting entirely.
// ============================================================================

using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using PlantSafety.Application.Abstractions.RateLimiting;
using PlantSafety.Domain.Billing.Subscription.Catalog;

namespace PlantSafety.Api.Extensions;

/// <summary>Extension methods registering real, per-tenant API rate limiting.</summary>
public static class RateLimitingExtensions
{
    /// <summary>The named rate limiter policy applied globally via <see cref="RateLimiterOptions.GlobalLimiter"/>.</summary>
    public const string PerTenantPolicyName = "PerTenant";

    private const string TenantIdClaimType = "tenant_id";

    /// <summary>Registers the per-tenant rate limiter (Phase 7 §3.4 License Enforcement).</summary>
    public static IServiceCollection AddPlantSafetyRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                PartitionedRateLimiter.Create<HttpContext, Guid>(httpContext =>
                {
                    var tenantId = ResolveTenantId(httpContext);
                    var cache = httpContext.RequestServices.GetRequiredService<ITenantRateLimitCache>();

                    var permitLimit = cache.TryGetRateLimitPerMinute(tenantId, out var cachedLimit)
                        ? cachedLimit
                        : SubscriptionPlanCatalog.Trial.Limits.ApiRateLimitPerMinute;

                    return RateLimitPartition.GetFixedWindowLimiter(tenantId, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                        AutoReplenishment = true,
                    });
                }));
        });

        return services;
    }

    /// <summary>Resolves the caller's tenant identifier for rate-limit partitioning from the <c>tenant_id</c> JWT claim, falling back to a shared "unauthenticated" partition.</summary>
    private static Guid ResolveTenantId(HttpContext httpContext)
    {
        var claimValue = httpContext.User.FindFirst(TenantIdClaimType)?.Value;
        return Guid.TryParse(claimValue, out var tenantId) ? tenantId : Guid.Empty;
    }
}
