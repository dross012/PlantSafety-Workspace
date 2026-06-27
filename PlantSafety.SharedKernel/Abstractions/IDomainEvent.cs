// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Abstractions
//  Layer:          SharedKernel
//  File:           IDomainEvent.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Lightweight domain event marker for the Abstractions layer.
//                  Domain layer events implement this; the richer envelope is in
//                  Events.Base.IDomainEvent. Additive-only evolution.
// ============================================================================
namespace PlantSafety.SharedKernel.Abstractions;

/// <summary>
/// Lightweight domain event marker used by the Abstractions layer.
/// Domain events MUST carry all state required to react to the event without
/// further queries. They MUST be immutable sealed records.
/// </summary>
public interface IDomainEvent
{
    /// <summary>Human-readable event type discriminator.</summary>
    string EventType { get; }

    /// <summary>Monotonically increasing version — starts at 1, incremented on breaking schema changes.</summary>
    int EventVersion { get; }

    /// <summary>Schema version for consumer compatibility checks (always "2026.1").</summary>
    string SchemaVersion { get; }

    /// <summary>UTC instant at which the domain event was raised.</summary>
    DateTime OccurredAtUtc { get; }
}
