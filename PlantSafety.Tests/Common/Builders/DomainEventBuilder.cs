// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Common
//  Layer:          Tests (Unit)
//  File:           DomainEventBuilder.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Assertion helpers for verifying domain event envelope fields in unit tests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
namespace PlantSafety.Tests.Common.Builders;

/// <summary>
/// Assertion helpers for verifying domain event envelopes raised by aggregates.
/// All assertions are deterministic and parameter-driven.
/// </summary>
public static class DomainEventAssertions
{
    /// <summary>
    /// Asserts that <paramref name="events"/> contains exactly one event of type
    /// <typeparamref name="T"/> and returns it for further assertions.
    /// </summary>
    public static T ShouldContainSingle<T>(IReadOnlyCollection<IDomainEvent> events)
        where T : IDomainEvent
    {
        events.Should().ContainSingle(e => e is T,
            $"aggregate should have raised exactly one {typeof(T).Name}");
        return (T)events.Single(e => e is T);
    }

    /// <summary>Asserts the event has a non-empty <c>SchemaVersion</c> of "2026.1".</summary>
    public static void ShouldHaveSchemaVersion2026(IDomainEvent evt)
    {
        evt.SchemaVersion.Should().Be("2026.1",
            "all domain events must declare SchemaVersion 2026.1");
    }

    /// <summary>Asserts the event version is at least 1.</summary>
    public static void ShouldHaveValidVersion(IDomainEvent evt)
    {
        evt.EventVersion.Should().BeGreaterThanOrEqualTo(1,
            "EventVersion must start at 1");
    }

    /// <summary>Asserts the event <c>OccurredAtUtc</c> equals <paramref name="expected"/>.</summary>
    public static void ShouldHaveOccurredAt(IDomainEvent evt, UtcTimestamp expected)
    {
        evt.OccurredAtUtc.Should().Be(expected,
            "OccurredAtUtc must match the timestamp passed to the factory method");
    }
}
