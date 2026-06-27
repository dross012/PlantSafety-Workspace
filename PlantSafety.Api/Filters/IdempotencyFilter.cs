// ============================================================================
// File: IdempotencyFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: IAsyncActionFilter that enforces idempotency for POST endpoints
//              decorated with [IdempotencyKey]. Checks an in-memory cache for a
//              previously stored response and short-circuits the action if found.
// ============================================================================
//
// Onboarding Notes:
// - Only POST endpoints decorated with [RequiresTenant] AND that carry the
//   Idempotency-Key request header are subject to this filter.
// - The idempotency cache key is: "idempotency:{tenantId}:{idempotencyKey}".
//   Using tenantId in the key prevents cross-tenant key collisions.
// - IMemoryCache is used for the cache store. In a multi-instance AKS deployment
//   this provides only node-local idempotency. Replace with IDistributedCache
//   (Redis) for cluster-wide idempotency guarantees once the Infrastructure layer
//   exposes a suitable cache abstraction.
// - Cache TTL is 24 hours, aligned with the client recommendation to not reuse
//   Idempotency-Key values beyond one day.
// - The filter is registered globally in ServiceCollectionExtensions but only
//   activates when the Idempotency-Key header is present. No header = no-op.
//
// Rationale:
// - Idempotent POST endpoints prevent duplicate submissions from network retries
//   and user double-clicks, critical for safety-regulated workflows in Compliance
//   and SafetyEvents modules.
//
// Future Extension Seams:
// - Replace IMemoryCache with IDistributedCache for cluster-wide guarantees.
// - Add a response status filter so only 2xx responses are cached (4xx errors
//   should be retryable with the same Idempotency-Key).
//
// ============================================================================

using Microsoft.Extensions.Caching.Memory;
using PlantSafety.Api.Constants;
using PlantSafety.Api.CrossDomain;

namespace PlantSafety.Api.Filters;

/// <summary>
/// Globally registered action filter enforcing idempotency for POST requests
/// that supply an <c>Idempotency-Key</c> header.
/// </summary>
public sealed class IdempotencyFilter : IAsyncActionFilter
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(24);

    private readonly IMemoryCache          _cache;
    private readonly TenantContextAccessor _tenantContext;
    private readonly ILogger<IdempotencyFilter> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="IdempotencyFilter"/>.
    /// </summary>
    public IdempotencyFilter(
        IMemoryCache              cache,
        TenantContextAccessor     tenantContext,
        ILogger<IdempotencyFilter> logger)
    {
        _cache         = cache;
        _tenantContext = tenantContext;
        _logger        = logger;
    }

    /// <inheritdoc />
    public async Task OnActionExecutionAsync(
        ActionExecutingContext  context,
        ActionExecutionDelegate next)
    {
        if (!HttpMethods.IsPost(context.HttpContext.Request.Method))
        {
            await next();
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(
                ApiHeaderConstants.IdempotencyKey, out var keyValues)
            || string.IsNullOrWhiteSpace(keyValues))
        {
            await next();
            return;
        }

        var idempotencyKey = keyValues.ToString();
        var cacheKey       = $"idempotency:{_tenantContext.TenantId}:{idempotencyKey}";

        if (_cache.TryGetValue(cacheKey, out IActionResult? cached) && cached is not null)
        {
            _logger.LogInformation(
                "Idempotency cache hit for key {IdempotencyKey} TenantId={TenantId}",
                idempotencyKey, _tenantContext.TenantId);
            context.Result = cached;
            return;
        }

        var executed = await next();

        if (executed.Result is ObjectResult { StatusCode: >= 200 and < 300 } result)
        {
            _cache.Set(cacheKey, result, CacheTtl);
        }
    }
}
