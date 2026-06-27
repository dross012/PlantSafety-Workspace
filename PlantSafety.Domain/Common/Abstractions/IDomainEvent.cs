// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Common
//  Layer:          Domain
//  File:           IDomainEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Canonical interface for all domain events in PlantSafety.Domain.
//      Every module-specific domain event interface (e.g. IAlertDomainEvent)
//      must ultimately satisfy this contract.
//      Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Common.Abstractions;

/// <summary>Base contract for all domain events raised by aggregate roots.</summary>
public interface IDomainEvent
{
    /// <summary>Discriminator string identifying the event kind (e.g. "AlertCreated").</summary>
    string EventType { get; }

    /// <summary>Schema version within this event kind (1-based, increment on breaking change).</summary>
    int EventVersion { get; }

    /// <summary>Cross-event schema edition (e.g. "2026.1"). Additive-only within an edition.</summary>
    string SchemaVersion { get; }

    /// <summary>UTC instant at which the domain action occurred.</summary>
    UtcTimestamp OccurredAtUtc { get; }
}
