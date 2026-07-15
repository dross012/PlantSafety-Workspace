// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Domain
//  File:           IProcessDefinitionDomainEvent.cs
//  Created:        2026-07-15
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all ProcessDefinition domain events.
//                  Carries the canonical event envelope required by §2.13.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ProcessManagement.Events;

/// <summary>Marker interface for domain events raised by the <c>ProcessDefinition</c> aggregate.</summary>
public interface IProcessDefinitionDomainEvent
{
    /// <summary>Canonical event type discriminator.</summary>
    string EventType { get; }

    /// <summary>Additive event schema version.</summary>
    int EventVersion { get; }

    /// <summary>Domain schema version this event was authored against.</summary>
    string SchemaVersion { get; }

    /// <summary>UTC timestamp the event occurred at.</summary>
    UtcTimestamp OccurredAtUtc { get; }
}
