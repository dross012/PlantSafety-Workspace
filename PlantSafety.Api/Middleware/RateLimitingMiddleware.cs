// ============================================================================
// File: RateLimitingMiddleware.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: ASP.NET Core middleware implementing a sliding-window per-tenant
//              rate limiter using IMemoryCache as the backing store. Returns
//              429 Too Many Requests when the limit is exceeded.
// ============================================================================
//
// Onboarding Notes:
// - Registered in UsePlantSafetyMiddleware() after UseAuthentication / UseAuthorization
//   so TenantId is available for per-tenant rate key construction.
// - Default limit: 300 requests per 60-second window per tenant. Override via
//   ApiOptions:RateLimit:RequestsPerMinute in appsettings.json or Key Vault.
// - The rate key is "ratelimit:{tenantId}" so limits are tenant-scoped, not
//   per-IP. This is appropriate for a B2B SaaS platform where all users in a
//   tenant share a single API quota negotiated in their SLA.
// - On 429, sets Retry-After header to the seconds remaining in the current window.
// - In production AKS, IMemoryCache is node-local only. For cluster-wide rate
//   limiting, replace with the .NET 8 built-in RateLimiter backed by Redis via
//   the Infrastructure layer. This implementation is the Presentation-layer stub.
//
// Rationale:
// - Prevents noisy-tenant abuse from degrading the shared API for all tenants,
//   a requirement from the PlantSafety SLA targets (§ Infrastructure & Security).
//
// Future Extension Seams:
// - Migrate to Microsoft.AspNetCore.RateLimiting (built-in .NET 8) with a Redis
//   store once Infrastructure exposes a suitable sliding-window policy factory.
// - Add per-route override: some endpoints (exports) may warrant tighter limits.
//
// ============================================================================

using Microsoft.Extensions.Caching.Memory;
using PlantSafety.Api.Configuration;
using PlantSafety.Api.CrossDomain;

namespace PlantSafety.Api.Middleware;

/// <summary>
/// Middleware enforcing per-tenant sliding-window rate limits.
/// Returns HTTP 429 with a <c>Retry-After</c> header when the limit is exceeded.
/// </summary>
public sealed class RateLimitingMiddleware
{
    private readonly RequestDelegate      _next;
    private readonly IMemoryCache         _cache;
    private readonly ApiOptions           _options;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="RateLimitingMiddleware"/>.
    /// </summary>
    public RateLimitingMiddleware(
        RequestDelegate               next,
        IMemoryCache                  cache,
        IOptions<ApiOptions>          options,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next    = next;
        _cache   = cache;
        _options = options.Value;
        _logger  = logger;
    }

    /// <summary>Invokes the rate-limiting check before delegating to the next middleware.</summary>
    /// <param name="context">Current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var tenantId = context.Items[TenantContextAccessor.TenantIdKey] as Guid? ?? Guid.Empty;

        if (tenantId == Guid.Empty)
        {
            await _next(context);
            return;
        }

        var windowSeconds = _options.RateLimit.WindowSeconds;
        var maxRequests   = _options.RateLimit.RequestsPerWindow;
        var cacheKey      = $"ratelimit:{tenantId}";

        var entry = _cache.GetOrCreate(cacheKey, ce =>
        {
            ce.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(windowSeconds);
            return new RateLimitEntry(0, DateTimeOffset.UtcNow.AddSeconds(windowSeconds));
        })!;

        entry = entry with { Count = entry.Count + 1 };
        _cache.Set(cacheKey, entry, entry.WindowExpiry);

        if (entry.Count > maxRequests)
        {
            var retryAfter = (int)Math.Ceiling((entry.WindowExpiry - DateTimeOffset.UtcNow).TotalSeconds);
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["Retry-After"] = retryAfter.ToString();

            _logger.LogWarning(
                "Rate limit exceeded for TenantId={TenantId}. Count={Count} Limit={Limit}",
                tenantId, entry.Count, maxRequests);

            await context.Response.WriteAsJsonAsync(new
            {
                status = 429,
                title  = "Too Many Requests",
                detail = $"Rate limit of {maxRequests} requests per {windowSeconds}s exceeded. " +
                         $"Retry after {retryAfter}s."
            });
            return;
        }

        await _next(context);
    }

    private sealed record RateLimitEntry(int Count, DateTimeOffset WindowExpiry);
}

/// <summary>Extension methods for registering <see cref="RateLimitingMiddleware"/>.</summary>
public static class RateLimitingMiddlewareExtensions
{
    /// <summary>Adds <see cref="RateLimitingMiddleware"/> to the pipeline.</summary>
    public static IApplicationBuilder UsePlantSafetyRateLimiting(this IApplicationBuilder builder)
        => builder.UseMiddleware<RateLimitingMiddleware>();
}
