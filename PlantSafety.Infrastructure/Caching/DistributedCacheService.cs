// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Caching
//  Layer:          Infrastructure
//  File:           DistributedCacheService.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    ICacheService backed by IDistributedCache (Redis / Azure Cache for Redis).
//                  Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.Caching.Distributed;

namespace PlantSafety.Infrastructure.Caching;

/// <summary>
/// <see cref="ICacheService"/> backed by <see cref="IDistributedCache"/> (Redis / Azure Cache for Redis).
/// All values are JSON-serialised using <see cref="JsonSerializerConfiguration.Default"/>.
/// </summary>
public sealed class DistributedCacheService(
    IDistributedCache cache,
    IOptions<CacheOptions> opts,
    ILogger<DistributedCacheService> logger)
    : ICacheService
{
    private readonly string _prefix = opts.Value.KeyPrefix;

    private string Key(string key) => $"{_prefix}{key}";

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var bytes = await cache.GetAsync(Key(key), cancellationToken);
        if (bytes is null) return default;
        return JsonSerializer.Deserialize<T>(bytes, JsonSerializerConfiguration.Default);
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var bytes  = JsonSerializer.SerializeToUtf8Bytes(value, JsonSerializerConfiguration.Default);
        var cacheOpts = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl };
        await cache.SetAsync(Key(key), bytes, cacheOpts, cancellationToken);
    }

    /// <inheritdoc/>
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => cache.RemoveAsync(Key(key), cancellationToken);

    /// <inheritdoc/>
    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null) return cached;

        var value = await factory();
        await SetAsync(key, value, ttl, cancellationToken);
        return value;
    }
}
