// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Caching
//  Layer:          Infrastructure
//  File:           ICacheService.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Abstraction over distributed cache operations. Explicit, bounded TTL always required.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Caching;

/// <summary>
/// Abstraction over distributed cache operations.
/// All operations require an explicit TTL — unbounded caching is prohibited.
/// </summary>
public interface ICacheService
{
    /// <summary>Gets the cached value for <paramref name="key"/>, or <see langword="null"/> if absent.</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>Sets the value for <paramref name="key"/> with an explicit <paramref name="ttl"/>.</summary>
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default);

    /// <summary>Removes the entry for <paramref name="key"/>.</summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a cached value or executes <paramref name="factory"/> to populate it.
    /// </summary>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan ttl, CancellationToken cancellationToken = default);
}
