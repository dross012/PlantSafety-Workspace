// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Common
//  Layer:          Tests (Unit)
//  File:           FakeDomainEventDispatcher.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    In-memory fake domain event dispatcher that captures raised events for unit test assertions.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
namespace PlantSafety.Tests.Common.Fakes;

/// <summary>
/// In-memory fake that captures all domain events raised during a unit test.
/// Allows assertions on event type, count, order, and field values without
/// a real message bus or dispatcher.
/// </summary>
public sealed class FakeDomainEventDispatcher
{
    private readonly List<IDomainEvent> _captured = [];

    /// <summary>All events captured since the last <see cref="Clear"/>.</summary>
    public IReadOnlyList<IDomainEvent> Captured => _captured.AsReadOnly();

    /// <summary>Records a domain event.</summary>
    public void Dispatch(IDomainEvent domainEvent) => _captured.Add(domainEvent);

    /// <summary>Records all events from an aggregate and clears its event list.</summary>
    public void DispatchAndClear(IHasDomainEvents aggregate)
    {
        foreach (var evt in aggregate.DomainEvents)
            _captured.Add(evt);
        aggregate.ClearDomainEvents();
    }

    /// <summary>Returns all captured events of type <typeparamref name="T"/>.</summary>
    public IReadOnlyList<T> Of<T>() where T : IDomainEvent
        => _captured.OfType<T>().ToList().AsReadOnly();

    /// <summary>Asserts that exactly one event of type <typeparamref name="T"/> was captured.</summary>
    public T Single<T>() where T : IDomainEvent
    {
        _captured.Should().ContainSingle(e => e is T,
            $"expected exactly one {typeof(T).Name}");
        return (T)_captured.Single(e => e is T);
    }

    /// <summary>Clears all captured events.</summary>
    public void Clear() => _captured.Clear();
}
