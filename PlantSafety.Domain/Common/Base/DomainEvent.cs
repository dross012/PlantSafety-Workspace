// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Common
//  Layer:          Domain
//  File:           DomainEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Optional abstract base record for domain events. Module-specific events
//      may inherit from this type or implement IDomainEvent directly.
//      Provides default SchemaVersion "2026.1" for all editions.
//      Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Common.Base;

/// <summary>
/// Optional base record for domain events. Supplies the canonical
/// <see cref="SchemaVersion"/> and requires subclasses to declare
/// <see cref="EventType"/>, <see cref="EventVersion"/>, and <see cref="OccurredAtUtc"/>.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    /// <inheritdoc/>
    public abstract string EventType { get; }

    /// <inheritdoc/>
    public abstract int EventVersion { get; }

    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";

    /// <inheritdoc/>
    public abstract UtcTimestamp OccurredAtUtc { get; }
}
