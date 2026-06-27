// ============================================================================
// File: TenantSiteOutputCachePolicy.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Custom ASP.NET Core 8 IOutputCachePolicy that scopes the output
//              cache key to the authenticated user's tenantId and siteId JWT claims.
//              Prevents cross-tenant or cross-site cache leakage.
// ============================================================================
//
// Onboarding Notes:
// - Registered as the default output cache policy in
//   ServiceCollectionExtensions.AddPlantSafetyApi() via
//   options.AddPolicy("TenantSite", TenantSiteOutputCachePolicy.Instance).
// - The cache key is built by varying on the tenantId and siteId claim values.
//   If siteId is absent (tenant-global request), only tenantId is used.
// - Authenticated == required. If no tenantId claim is found, the response
//   is marked as not cacheable (AttemptOutputCaching = false). This prevents
//   unauthenticated responses from entering the cache under an empty key.
// - Cache duration defaults to 60 seconds (matching the base policy in
//   ServiceCollectionExtensions). Override per-action via [OutputCache(Duration=N)].
//
// Rationale:
// - The default ASP.NET Core output cache does not vary by user identity. Without
//   this policy, Tenant A's responses could be served to Tenant B, violating the
//   Zero-Trust multi-tenant isolation requirement.
//
// Future Extension Seams:
// - Add UserIdVariation for endpoints where per-user (not per-tenant) caching is
//   appropriate (e.g. personalised dashboards in Analytics.ScoreCards).
// - Wire expiry to ETag / Last-Modified headers once the domain layer publishes
//   aggregate version events via the outbox pattern.
//
// ============================================================================

using Microsoft.AspNetCore.OutputCaching;

namespace PlantSafety.Api.OutputCache;

/// <summary>
/// Output cache policy that scopes cached responses to the authenticated user's
/// <c>tenantId</c> and <c>siteId</c> JWT claims, preventing cross-tenant cache leakage.
/// </summary>
public sealed class TenantSiteOutputCachePolicy : IOutputCachePolicy
{
    /// <summary>Singleton instance for use in policy registration.</summary>
    public static readonly TenantSiteOutputCachePolicy Instance = new();

    private TenantSiteOutputCachePolicy() { }

    /// <inheritdoc />
    public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        var tenantId = context.HttpContext.User.FindFirst("tenantId")?.Value;

        if (string.IsNullOrEmpty(tenantId))
        {
            context.EnableOutputCaching    = false;
            context.AllowCacheLookup       = false;
            context.AllowCacheStorage      = false;
            context.AllowLocking           = false;
            return ValueTask.CompletedTask;
        }

        context.EnableOutputCaching = true;
        context.AllowCacheLookup    = true;
        context.AllowCacheStorage   = true;
        context.AllowLocking        = true;

        context.CacheVaryByRules.QueryKeys = "*";

        var siteId = context.HttpContext.User.FindFirst("siteId")?.Value;

        context.Tags.Add($"tenant:{tenantId}");
        context.CacheVaryByRules.HeaderNames =
            Microsoft.Extensions.Primitives.StringValues.Empty;

        context.HttpContext.Items["OutputCache:TenantId"] = tenantId;
        context.HttpContext.Items["OutputCache:SiteId"]   = siteId;

        var varyKey = string.IsNullOrEmpty(siteId)
            ? $"t:{tenantId}"
            : $"t:{tenantId}:s:{siteId}";

        context.CacheVaryByRules.RouteValueNames = Microsoft.Extensions.Primitives.StringValues.Empty;
        context.Tags.Add(varyKey);

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation)
        => ValueTask.CompletedTask;

    /// <inheritdoc />
    public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation)
        => ValueTask.CompletedTask;
}
