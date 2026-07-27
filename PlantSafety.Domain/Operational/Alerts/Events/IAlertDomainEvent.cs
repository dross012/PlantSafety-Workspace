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
    AlertEventId EventId { get; }
    string EventType { get; }
    int EventVersion { get; }
    string SchemaVersion { get; }
    UtcTimestamp OccurredAtUtc { get; }
    CorrelationId CorrelationId { get; }
    CausationId CausationId { get; }
    PerformedByActor Actor { get; }
}

