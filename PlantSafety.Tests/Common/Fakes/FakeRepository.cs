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
using System.Reflection;

namespace PlantSafety.Tests.Common.Fakes;

/// <summary>
/// Generic in-memory fake repository for unit tests.
/// Stores aggregates by their identifier and simulates repository behaviour
/// without any database, file I/O, or network access.
/// The identifier is extracted automatically via reflection: the single public
/// instance property on <typeparamref name="TAggregate"/> whose type is
/// <typeparamref name="TId"/> (e.g. <c>Alert.AlertId</c>).
/// </summary>
/// <typeparam name="TAggregate">The aggregate root type.</typeparam>
/// <typeparam name="TId">The strong-typed identifier type.</typeparam>
public sealed class FakeRepository<TAggregate, TId>
    where TAggregate : class
    where TId : struct
{
    private static readonly PropertyInfo? IdProperty =
        typeof(TAggregate).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p => p.PropertyType == typeof(TId));

    private readonly Dictionary<TId, TAggregate> _store = new();

    /// <summary>All aggregates currently held in the fake store.</summary>
    public IReadOnlyCollection<TAggregate> All => _store.Values.ToList().AsReadOnly();

    /// <summary>Returns the aggregate for <paramref name="id"/>, or <c>null</c> if absent.</summary>
    public Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
        => Task.FromResult(_store.TryGetValue(id, out var agg) ? agg : null);

    /// <summary>Stores <paramref name="aggregate"/>, keyed by the identifier extracted from it.</summary>
    public Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default)
    {
        _store[ExtractId(aggregate)] = aggregate;
        return Task.CompletedTask;
    }

    /// <summary>Returns <c>true</c> if an aggregate with <paramref name="id"/> is stored.</summary>
    public bool Contains(TId id) => _store.ContainsKey(id);

    /// <summary>Total number of aggregates in the fake store.</summary>
    public int Count => _store.Count;

    /// <summary>Clears all stored aggregates.</summary>
    public void Clear() => _store.Clear();

    private static TId ExtractId(TAggregate aggregate)
    {
        if (IdProperty is null)
        {
            throw new InvalidOperationException(
                $"FakeRepository<{typeof(TAggregate).Name}, {typeof(TId).Name}> could not find a public " +
                $"property of type {typeof(TId).Name} on {typeof(TAggregate).Name} to use as the identifier.");
        }

        return (TId)IdProperty.GetValue(aggregate)!;
    }
}
