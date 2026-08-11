// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           IAlertDomainEvent.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Canonical domain event envelope for all Alerts domain events.
//      Complies with:
//          - Domain Events Architectural Rule (2.13)
//          - Event Envelope Rule (12.5)
//          - Event Versioning Rule (Part XVI)
//          - StrongId Enforcement Rule (5.5)
//          - Deterministic Time Rule (1.1)
//      Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Abstractions;

namespace PlantSafety.Domain.Operational.Alerts.Events;

public interface IAlertDomainEvent : IDomainEvent
{
    // EventType, EventVersion, SchemaVersion and OccurredAtUtc are NOT redeclared here.
    // They were, verbatim and with identical types, which hid the inherited IDomainEvent
    // members (CS0108) while adding nothing: an interface that re-states a member it already
    // inherits unchanged only creates two declarations to keep in step. Inheriting them means
    // a future change to IDomainEvent reaches this interface instead of silently diverging
    // from it.

    /// <summary>Strongly-typed alert event identifier. IDomainEvent declares no EventId, so this
    /// adds a member rather than hiding one.</summary>
    AlertEventId EventId { get; }

    CorrelationId CorrelationId { get; }
    CausationId CausationId { get; }
    PerformedByActor Actor { get; }
}

