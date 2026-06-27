// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Common
//  Layer:          Tests (Unit)
//  File:           FakeRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    In-memory fake repository for unit testing aggregate persistence without a database.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
namespace PlantSafety.Tests.Common.Fakes;

/// <summary>
/// Generic in-memory fake repository for unit tests.
/// Stores aggregates by their identifier and simulates repository behaviour
/// without any database, file I/O, or network access.
/// </summary>
/// <typeparam name="TAggregate">The aggregate root type.</typeparam>
/// <typeparam name="TId">The strong-typed identifier type.</typeparam>
public sealed class FakeRepository<TAggregate, TId>
    where TAggregate : class
    where TId : struct
{
    private readonly Dictionary<TId, TAggregate> _store = new();

    /// <summary>All aggregates currently held in the fake store.</summary>
    public IReadOnlyDictionary<TId, TAggregate> Store => _store;

    /// <summary>Returns the aggregate for <paramref name="id"/>, or <c>null</c> if absent.</summary>
    public Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
        => Task.FromResult(_store.TryGetValue(id, out var agg) ? agg : null);

    /// <summary>Stores the aggregate, keyed by <paramref name="id"/>.</summary>
    public Task AddAsync(TId id, TAggregate aggregate, CancellationToken cancellationToken = default)
    {
        _store[id] = aggregate;
        return Task.CompletedTask;
    }

    /// <summary>Returns <c>true</c> if an aggregate with <paramref name="id"/> is stored.</summary>
    public bool Contains(TId id) => _store.ContainsKey(id);

    /// <summary>Total number of aggregates in the fake store.</summary>
    public int Count => _store.Count;

    /// <summary>Clears all stored aggregates.</summary>
    public void Clear() => _store.Clear();
}
