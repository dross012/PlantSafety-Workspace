// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Caching
//  Layer:          Infrastructure
//  File:           ApplicationCacheServiceAdapter.cs
//  Created:        2026-07-21 (Claude Code -- platform-wide DI-collision bug fix)
//  Description:    Implementation of PlantSafety.Application.Abstractions.Caching.ICacheService
//                  -- the interface actually consumed by CachingBehavior<,>, which wraps EVERY
//                  MediatR request (queries and commands alike; see ApplicationServiceRegistration
//                  pipeline order).
//
//  BUG THIS FIXES (found during verification of the IUnitOfWork/outbox fix, same root cause):
//  PlantSafety.Application.Abstractions.Caching.ICacheService and PlantSafety.Infrastructure.
//  Caching.ICacheService are two DIFFERENTLY SHAPED interfaces with the same simple name.
//  InfrastructureServiceRegistration only ever registered
//      services.AddScoped&lt;ICacheService, DistributedCacheService&gt;();
//  which -- because that file has `using PlantSafety.Infrastructure.Caching;` -- resolves to the
//  Infrastructure interface, NOT the Application one CachingBehavior&lt;,&gt; depends on. The
//  Application interface had zero implementation and zero registration, so CachingBehavior&lt;,&gt;
//  could never be constructed for ANY request (query or command) -- i.e. the MediatR pipeline
//  could never run at all, for any module, independent of the separate IUnitOfWork bug. This
//  class + its fully-qualified DI registration (see InfrastructureServiceRegistration.cs) closes
//  that second gap.
//
//  KNOWN LIMITATION: RemoveByPrefixAsync uses IDatabase.KEYS-style pattern scanning via
//  StackExchange.Redis's SERVER SCAN, which requires direct IConnectionMultiplexer access (not
//  exposed through IDistributedCache). Implemented via IConnectionMultiplexer directly; if Redis
//  is unavailable in a given environment this call will throw like any other Redis-dependent
//  operation -- unchanged from the pre-existing caching behavior for Get/Set/Remove.
//                  Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using AppCacheAbstractions = PlantSafety.Application.Abstractions.Caching;

namespace PlantSafety.Infrastructure.Caching;

/// <summary>
/// <see cref="AppCacheAbstractions.ICacheService"/> backed by <see cref="IDistributedCache"/>
/// (Redis / Azure Cache for Redis). Consumed by <c>CachingBehavior&lt;,&gt;</c>.
/// </summary>
public sealed class ApplicationCacheServiceAdapter(
    IDistributedCache cache,
    IOptions<CacheOptions> opts,
    ILogger<ApplicationCacheServiceAdapter> logger,
    IConnectionMultiplexer? redis = null)
    : AppCacheAbstractions.ICacheService
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
    public async Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonSerializerConfiguration.Default);
        var cacheOpts = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiry };
        await cache.SetAsync(Key(key), bytes, cacheOpts, cancellationToken);
    }

    /// <inheritdoc/>
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => cache.RemoveAsync(Key(key), cancellationToken);

    /// <inheritdoc/>
    public async Task RemoveByPrefixAsync(string keyPrefix, CancellationToken cancellationToken = default)
    {
        if (redis is null)
        {
            logger.LogWarning("RemoveByPrefixAsync: no IConnectionMultiplexer configured -- skipping prefix invalidation for {KeyPrefix}.", keyPrefix);
            return;
        }

        var fullPrefix = Key(keyPrefix);
        foreach (var endpoint in redis.GetEndPoints())
        {
            var server = redis.GetServer(endpoint);
            await foreach (var redisKey in server.KeysAsync(pattern: $"{fullPrefix}*"))
            {
                await redis.GetDatabase().KeyDeleteAsync(redisKey);
            }
        }
    }

    /// <inheritdoc/>
    public async Task<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null) return cached;

        var value = await factory(cancellationToken);
        await SetAsync(key, value, expiry, cancellationToken);
        return value;
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        => await cache.GetAsync(Key(key), cancellationToken) is not null;
}
