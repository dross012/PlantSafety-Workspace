// ============================================================================
//  File:           AlertAcknowledgedEvent.cs
//  Created:        2026-05-26
//  SchemaVersion:  2026.1
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Events;

public sealed record AlertAcknowledgedEvent(
    AlertEventId EventId,
    AlertId AlertId,
    UtcTimestamp AcknowledgedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAlertDomainEvent
{
    public string EventType => "AlertAcknowledged";
    public int EventVersion => 1;
    public string SchemaVersion => "2026.1";
    public UtcTimestamp OccurredAtUtc => AcknowledgedAtUtc;
}
