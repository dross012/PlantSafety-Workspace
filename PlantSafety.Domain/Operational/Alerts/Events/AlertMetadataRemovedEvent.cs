// =====================================================================================================
//  FILE: AlertMetadataRemovedEvent.cs
//  MODULE: Operational.Alerts
//  LAYER: Domain
//  SCHEMA VERSION: 2026.1
//  AUTHOR: David (Ted) + Copilot
//  CREATED: 2026-05-27
//
//  PURPOSE:
//      Domain event representing the removal of a metadata entry from an Alert. This event is
//      raised when metadata is explicitly removed by a command handler or domain operation.
//      The event is additive-only in the event stream (Rule 7.1) and is used during replay,
//      rebuild, and projection updates.
//
//  RATIONALE & ONBOARDING NOTES:
//      - Domain events MUST be immutable (Rule 2.13).
//      - Domain events MUST contain only data, no behavior.
//      - Domain events MUST include correlation + causation identifiers.
//      - Domain events MUST be replay-safe and deterministic.
//      - Metadata removal is a structural mutation and MUST NOT contain business logic.
//      - This event is consumed by:
//          * Alert.ApplyMetadataRemoved()
//          * Projections
//          * Materialized views
//          * Audit logs
//          * Rebuild pipelines
//
//  ENTERPRISE HANDBOOK COMPLIANCE:
//      ✔ Immutable record type
//      ✔ Strong IDs (AlertId, AlertEventId)
//      ✔ UtcTimestamp for all temporal fields
//      ✔ Actor identity included
//      ✔ Correlation + Causation IDs included
//      ✔ No domain logic
//
// =====================================================================================================
namespace PlantSafety.Domain.Operational.Alerts.Events;

/// <summary>
/// Domain event representing the removal of a metadata entry from an Alert.
/// </summary>
public sealed record AlertMetadataRemovedEvent(
    AlertEventId EventId,
    AlertId AlertId,
    AlertMetadataKey Key,
    UtcTimestamp RemovedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAlertDomainEvent
{
    public string EventType => "AlertMetadataRemoved";
    public int EventVersion => 1;
    public string SchemaVersion => "2026.1";
    public UtcTimestamp OccurredAtUtc => RemovedAtUtc;
}

