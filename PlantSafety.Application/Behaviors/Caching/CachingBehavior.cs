// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: MediatR pipeline behavior providing transparent caching for queries
//   that implement ICacheableQuery. Only IQuery requests that opt in are cached.
//   Commands are never cached. Cache keys MUST be tenant-namespaced for isolation.
// SchemaVersion: 1
// Additive-only evolution: new fields may be added; no caching semantics may be altered destructively.

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Caching;
using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Behaviors.Caching;

/// <summary>
/// MediatR pipeline behavior that serves cacheable query results from the distributed cache.
/// Only queries implementing <see cref="ICacheableQuery"/> participate; all others pass through.
/// Cache keys are always tenant-namespaced to enforce multi-tenancy isolation.
/// </summary>
/// <typeparam name="TRequest">The MediatR request type.</typeparam>
/// <typeparam name="TResponse">The response type produced by the handler.</typeparam>
public sealed class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="CachingBehavior{TRequest,TResponse}"/>.
    /// </summary>
    /// <param name="cacheService">The distributed cache service.</param>
    /// <param name="logger">The structured logger.</param>
    public CachingBehavior(ICacheService cacheService, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Checks the cache for cacheable queries and returns the cached result on hit.
    /// Non-cacheable requests or Commands pass directly to the next delegate.
    /// </summary>
    /// <param name="request">The incoming MediatR request.</param>
    /// <param name="next">The delegate to the next behavior or final handler.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>The cached or freshly computed response.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery cacheable)
        {
            return await next();
        }

        var cached = await _cacheService.GetAsync<TResponse>(cacheable.CacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogDebug("CacheHit | CacheKey={CacheKey}", cacheable.CacheKey);
            return cached;
        }

        var response = await next();

        if (response is not null)
        {
            await _cacheService.SetAsync(cacheable.CacheKey, response, cacheable.CacheExpiry, cancellationToken);
            _logger.LogDebug("CacheSet | CacheKey={CacheKey} | Expiry={Expiry}", cacheable.CacheKey, cacheable.CacheExpiry);
        }

        return response;
    }
}

/// <summary>
/// Opt-in marker interface for queries that should be cached.
/// Implementing this interface on a query causes the <see cref="CachingBehavior{TRequest,TResponse}"/>
/// to check and populate the distributed cache.
/// </summary>
public interface ICacheableQuery
{
    /// <summary>Gets the tenant-namespaced cache key for this query result.</summary>
    string CacheKey { get; }

    /// <summary>Gets the absolute cache expiry duration for this result.</summary>
    TimeSpan CacheExpiry { get; }
}
