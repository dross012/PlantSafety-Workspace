// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Common
//  Layer:          Tests (Unit)
//  File:           DomainEventExtensions.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    FluentAssertions extension methods for domain event collections.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
namespace PlantSafety.Tests.Common.Extensions;

/// <summary>
/// FluentAssertions-style extension methods for asserting domain event collections
/// raised by aggregate roots.
/// </summary>
public static class DomainEventExtensions
{
    /// <summary>
    /// Asserts that the aggregate raised exactly one event of type
    /// <typeparamref name="TEvent"/> and returns it.
    /// </summary>
    public static TEvent ShouldRaiseSingleEvent<TEvent>(
        this IReadOnlyCollection<IDomainEvent> events)
        where TEvent : IDomainEvent
    {
        events.Should().ContainSingle(
            e => e is TEvent,
            $"aggregate should raise exactly one {typeof(TEvent).Name}");
        return (TEvent)events.Single(e => e is TEvent);
    }

    /// <summary>Asserts the event has SchemaVersion "2026.1".</summary>
    public static TEvent WithSchemaVersion2026<TEvent>(this TEvent evt)
        where TEvent : IDomainEvent
    {
        evt.SchemaVersion.Should().Be("2026.1");
        return evt;
    }

    /// <summary>Asserts the event occurred at the expected timestamp.</summary>
    public static TEvent WithOccurredAt<TEvent>(this TEvent evt, UtcTimestamp expected)
        where TEvent : IDomainEvent
    {
        evt.OccurredAtUtc.Should().Be(expected);
        return evt;
    }
}
