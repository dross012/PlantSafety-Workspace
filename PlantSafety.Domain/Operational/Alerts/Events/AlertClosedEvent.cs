// ============================================================================
//  File:           AlertClosedEvent.cs
//  Created:        2026-05-26
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Events;

public sealed record AlertClosedEvent(
    AlertEventId EventId,
    AlertId AlertId,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAlertDomainEvent
{
    public string EventType => "AlertClosed";
    public int EventVersion => 1;
    public string SchemaVersion => "2026.1";
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}
