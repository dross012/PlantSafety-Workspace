// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Application-layer cache abstraction backed by Azure Redis Cache.
//   Provides deterministic, tenant-isolated caching for read-side query results.
//   Cache keys must always include TenantId to prevent cross-tenant data leakage.
// SchemaVersion: 1
// Additive-only evolution: new members may be added; no members may be removed or renamed.

namespace PlantSafety.Application.Abstractions.Caching;

/// <summary>
/// Abstracts distributed cache operations for the Application layer.
/// All cache keys MUST be namespaced with TenantId to enforce multi-tenancy isolation.
/// Infrastructure provides the Azure Redis implementation.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Retrieves a cached value by key.
    /// Returns <c>null</c> on cache miss; never throws on miss.
    /// </summary>
    /// <typeparam name="T">The deserialized type of the cached value.</typeparam>
    /// <param name="key">The fully-qualified, tenant-namespaced cache key.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>The cached value, or <c>null</c> if not found.</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a value in the cache with an absolute expiry.
    /// </summary>
    /// <typeparam name="T">The type of value to cache.</typeparam>
    /// <param name="key">The fully-qualified, tenant-namespaced cache key.</param>
    /// <param name="value">The value to store.</param>
    /// <param name="expiry">Absolute expiry duration. Must be a positive value.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a specific cache entry by key.
    /// Does not throw if the key does not exist.
    /// </summary>
    /// <param name="key">The fully-qualified cache key to remove.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all cache entries matching a prefix pattern (used for aggregate-level cache invalidation).
    /// </summary>
    /// <param name="keyPrefix">The key prefix to match for bulk removal.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task RemoveByPrefixAsync(string keyPrefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a value from cache, or executes the factory and caches the result.
    /// The factory is only called on a cache miss.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The fully-qualified, tenant-namespaced cache key.</param>
    /// <param name="factory">The async factory to execute on cache miss.</param>
    /// <param name="expiry">Absolute expiry duration for the cached value.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>The cached or freshly fetched value.</returns>
    Task<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan expiry, CancellationToken cancellationToken = default);

    /// <summary>Returns <c>true</c> if a cache entry exists for the given key.</summary>
    /// <param name="key">The fully-qualified cache key.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}
