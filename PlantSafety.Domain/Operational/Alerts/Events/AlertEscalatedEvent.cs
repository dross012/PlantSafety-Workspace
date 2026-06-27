// ============================================================================
//  File:           AlertEscalatedEvent.cs
//  Created:        2026-05-26
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Events;

public sealed record AlertEscalatedEvent(
    AlertEventId EventId,
    AlertId AlertId,
    EscalationReason Reason,
    UtcTimestamp EscalatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAlertDomainEvent
{
    public string EventType => "AlertEscalated";
    public int EventVersion => 1;
    public string SchemaVersion => "2026.1";
    public UtcTimestamp OccurredAtUtc => EscalatedAtUtc;
}
